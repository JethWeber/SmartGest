using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Cliente HTTP central do SmartGest Desktop.
///
/// Funcionalidades:
///   • JWT automático via TokenStore
///   • Retry com backoff exponencial para falhas transitórias (429, 502, 503, 504)
///   • CancellationToken em todos os métodos públicos
///   • Leitura de erros robusta (JSON → texto plano → ReasonPhrase)
///   • Logging estruturado via System.Diagnostics.Trace
/// </summary>
public class ApiClient
{
    private readonly HttpClient  _http;
    private readonly TokenStore  _store;

    private const int    MaxRetries        = 3;
    private const double RetryBaseDelayMs  = 300; // ms — duplica a cada tentativa

    // Injeção do HttpClient pelo DI é a forma recomendada. Mantemos um
    // construtor de fallback (usado apenas pelo Avalonia Designer) que cria
    // um HttpClient com a BaseAddress padrão.
    public ApiClient(HttpClient http, TokenStore store)
    {
        _http  = http ?? throw new ArgumentNullException(nameof(http));
        _store = store ?? throw new ArgumentNullException(nameof(store));

        Trace.TraceInformation(
            $"[ApiClient] Inicializado. BaseUrl={_http.BaseAddress} TokenStore=#{store.GetHashCode()}");
    }

    // Fallback conveniente para designers/testes locais — usa o URL padrão.
    public ApiClient(TokenStore store) : this(new HttpClient
    {
        BaseAddress = new Uri("http://localhost:8080/"),
        Timeout     = Timeout.InfiniteTimeSpan
    }, store) { }

    // ══════════════════════════════════════════════════════════════════════════
    // API pública
    // ══════════════════════════════════════════════════════════════════════════

    public Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Get, url, body: null, ct);

    public Task<T?> PostAsync<T>(string url, object body, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Post, url, body, ct);

    public Task PostAsync(string url, object body, CancellationToken ct = default)
        => SendAsync<object>(HttpMethod.Post, url, body, ct);

    public Task<T?> PutAsync<T>(string url, object body, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Put, url, body, ct);

    public Task<T?> PatchAsync<T>(string url, object body, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Patch, url, body, ct);

    public Task DeleteAsync(string url, CancellationToken ct = default)
        => SendAsync<object>(HttpMethod.Delete, url, body: null, ct);

    // ══════════════════════════════════════════════════════════════════════════
    // Motor interno com retry
    // ══════════════════════════════════════════════════════════════════════════

    private async Task<T?> SendAsync<T>(
        HttpMethod method, string url, object? body, CancellationToken ct)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            // Timeout de 30 s por pedido, respeitando também o ct do chamador
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linkedCts  = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            HttpResponseMessage? resp = null;
            var sw = Stopwatch.StartNew();

            try
            {
                using var req = BuildRequest(method, url, body);
                resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);
                sw.Stop();

                Log(method, url, resp.StatusCode, sw.ElapsedMilliseconds, attempt);

                // ── Sucesso ───────────────────────────────────────────────────
                if (resp.IsSuccessStatusCode)
                {
                    // DELETE / void — sem corpo
                    if (typeof(T) == typeof(object) || resp.StatusCode == HttpStatusCode.NoContent)
                        return default;

                    return await resp.Content.ReadFromJsonAsync<T>(JsonOptions.Default, ct);
                }

                // ── Erro retryable (throttle / gateway) ───────────────────────
                if (IsRetryable(resp.StatusCode) && attempt < MaxRetries)
                {
                    var delay = TimeSpan.FromMilliseconds(RetryBaseDelayMs * Math.Pow(2, attempt - 1));
                    Trace.TraceWarning(
                        $"[ApiClient] {method} {url} → {(int)resp.StatusCode} — retry {attempt}/{MaxRetries} em {delay.TotalMilliseconds:F0} ms");
                    await Task.Delay(delay, ct);
                    continue;
                }

                // ── Erro definitivo ────────────────────────────────────────────
                await ThrowApiExceptionAsync(resp);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                Trace.TraceError($"[ApiClient] {method} {url} — TIMEOUT (attempt {attempt})");
                if (attempt == MaxRetries)
                    throw new ApiException(HttpStatusCode.RequestTimeout,
                        "O pedido excedeu o tempo limite (30 s). Verifique a ligação à API.");

                await Task.Delay(TimeSpan.FromMilliseconds(RetryBaseDelayMs * attempt), ct);
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries)
            {
                // Sem ligação de rede — tenta novamente
                Trace.TraceWarning($"[ApiClient] {method} {url} — rede: {ex.Message} (retry {attempt})");
                await Task.Delay(TimeSpan.FromMilliseconds(RetryBaseDelayMs * attempt), ct);
            }
            finally
            {
                resp?.Dispose();
            }
        }

        // Não deve chegar aqui (o loop sempre lança ou retorna)
        throw new ApiException(HttpStatusCode.InternalServerError, "Erro inesperado no cliente HTTP.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Helpers privados
    // ══════════════════════════════════════════════════════════════════════════

    private HttpRequestMessage BuildRequest(HttpMethod method, string url, object? body)
    {
        var req = new HttpRequestMessage(method, url);

        if (!string.IsNullOrEmpty(_store.Token))
            req.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _store.Token);

        if (body is not null)
            req.Content = JsonContent.Create(body, options: JsonOptions.Default);

        return req;
    }

    private static bool IsRetryable(HttpStatusCode code) => code is
        HttpStatusCode.TooManyRequests  or   // 429
        HttpStatusCode.BadGateway       or   // 502
        HttpStatusCode.ServiceUnavailable or // 503
        HttpStatusCode.GatewayTimeout;       // 504

    private static async Task ThrowApiExceptionAsync(HttpResponseMessage resp)
    {
        string? mensagem = null;

        try
        {
            // Tenta ler o campo "message" que a API devolve nos erros
            var erro = await resp.Content.ReadFromJsonAsync<ApiErroResponse>(JsonOptions.Default);
            mensagem = erro?.Message ?? erro?.message;
        }
        catch
        {
            // Fallback para texto plano (ex.: HTML de erros de proxy)
            try { mensagem = (await resp.Content.ReadAsStringAsync()).Trim(); }
            catch { /* ignora */ }
        }

        if (string.IsNullOrWhiteSpace(mensagem))
            mensagem = resp.ReasonPhrase ?? "Erro desconhecido";

        throw new ApiException(resp.StatusCode, mensagem);
    }

    private static void Log(HttpMethod method, string url, HttpStatusCode status, long ms, int attempt)
    {
        var msg = $"[ApiClient] {method,-6} {url,-55} → {(int)status} {status,-20} {ms,4} ms";
        if (attempt > 1) msg += $" (attempt {attempt})";

        if ((int)status >= 500)      Trace.TraceError(msg);
        else if ((int)status >= 400) Trace.TraceWarning(msg);
        else                         Trace.TraceInformation(msg);
    }
}

// ── Tipos auxiliares ──────────────────────────────────────────────────────────

internal record ApiErroResponse(string? Message = null, string? message = null);

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ApiException(HttpStatusCode code, string mensagem)
        : base(mensagem) => StatusCode = code;

    /// <summary>Verdadeiro para erros de autenticação/autorização.</summary>
    public bool IsUnauthorized => StatusCode is
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;

    /// <summary>Verdadeiro quando o recurso não foi encontrado.</summary>
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;

    /// <summary>Verdadeiro para conflitos (ex.: NIB duplicado).</summary>
    public bool IsConflict => StatusCode == HttpStatusCode.Conflict;
}

internal static class JsonOptions
{
    public static readonly System.Text.Json.JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive    = true,
        DefaultIgnoreCondition         =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
}
