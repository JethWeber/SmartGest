using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly TokenStore _store;

    public const string BaseUrl = "http://localhost:5275/";

    public ApiClient(TokenStore store)
    {
        _store = store;
        _http  = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout     = TimeSpan.FromSeconds(30)
        };

        // Diagnóstico: confirma qual TokenStore este ApiClient usa
        System.Diagnostics.Debug.WriteLine(
            $"[ApiClient] Criado. TokenStore.GetHashCode={store.GetHashCode()}");
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var req  = BuildRequest(HttpMethod.Get, url);
        var resp = await _http.SendAsync(req);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<T>(JsonOptions.Default);
    }

    public async Task<T?> PostAsync<T>(string url, object body)
    {
        var req  = BuildRequest(HttpMethod.Post, url);
        req.Content = JsonContent.Create(body, options: JsonOptions.Default);
        var resp = await _http.SendAsync(req);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<T>(JsonOptions.Default);
    }

    public async Task PostAsync(string url, object body)
    {
        var req  = BuildRequest(HttpMethod.Post, url);
        req.Content = JsonContent.Create(body, options: JsonOptions.Default);
        var resp = await _http.SendAsync(req);
        await EnsureSuccessAsync(resp);
    }

    public async Task<T?> PutAsync<T>(string url, object body)
    {
        var req  = BuildRequest(HttpMethod.Put, url);
        req.Content = JsonContent.Create(body, options: JsonOptions.Default);
        var resp = await _http.SendAsync(req);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<T>(JsonOptions.Default);
    }

    public async Task DeleteAsync(string url)
    {
        var req  = BuildRequest(HttpMethod.Delete, url);
        var resp = await _http.SendAsync(req);
        await EnsureSuccessAsync(resp);
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);

        // Diagnóstico: confirma o estado do token em cada pedido
        System.Diagnostics.Debug.WriteLine(
            $"[ApiClient] {method} {url} | TokenStore.GetHashCode={_store.GetHashCode()} | " +
            $"Token vazio={string.IsNullOrEmpty(_store.Token)}");

        if (!string.IsNullOrEmpty(_store.Token))
            req.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _store.Token);

        return req;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;

        string? mensagem = null;
        try
        {
            var erro = await resp.Content.ReadFromJsonAsync<ApiErroResponse>();
            mensagem = erro?.Message ?? erro?.message;
        }
        catch { }

        throw new ApiException(resp.StatusCode, mensagem ?? resp.ReasonPhrase ?? "Erro desconhecido");
    }
}

internal record ApiErroResponse(string? Message = null, string? message = null);

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public ApiException(HttpStatusCode code, string mensagem) : base(mensagem)
        => StatusCode = code;
}

internal static class JsonOptions
{
    public static readonly System.Text.Json.JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
}
