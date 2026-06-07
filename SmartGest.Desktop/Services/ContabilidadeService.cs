using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

// ── DTOs — Balancete ──────────────────────────────────────────────────────────

public record BalanceteItemResponse(
    string Codigo,
    string Nome,
    string Grupo,
    double SaldoAnteriorDebito,
    double SaldoAnteriorCredito,
    double MovDebito,
    double MovCredito,
    double SaldoFinalDebito,
    double SaldoFinalCredito);

public record BalanceteApiResponse(
    BalancetePeriodo Periodo,
    double           TotalDebitos,
    double           TotalCreditos,
    List<BalanceteItemResponse> Items);

public record BalancetePeriodo(DateTime Inicio, DateTime Fim);

// ── DTOs — Balanço ────────────────────────────────────────────────────────────

public record BalancoLinhaResponse(string Descricao, decimal Valor, bool IsDeducao = false);

public record BalancoApiResponse(
    List<BalancoLinhaResponse> AtivoCorrentes,
    List<BalancoLinhaResponse> AtivoNaoCorrentes,
    List<BalancoLinhaResponse> PassivosCorrentes,
    List<BalancoLinhaResponse> PassivosNaoCorrentes,
    List<BalancoLinhaResponse> CapitalProprio,
    decimal TotalAtivo,
    decimal TotalPassivo,
    decimal TotalCapital,
    decimal TotalPassivoMaisCapital);

// ── DTOs — DRE ────────────────────────────────────────────────────────────────

public record DreItemResponse(
    string   Codigo,
    string   Nome,
    string   Grupo,
    decimal  ValorOrcado,
    decimal  ValorRealizado,
    bool     IsReceita,
    DateTime DataOrigem);

public record DreSumarioApiResponse(
    decimal              TotalReceitas,
    decimal              TotalCustos,
    decimal              ResultadoLiquido,
    List<DreItemResponse> Linhas);

// ── Serviço ───────────────────────────────────────────────────────────────────

public class ContabilidadeService
{
    private readonly ApiClient _api;

    public ContabilidadeService(ApiClient api) => _api = api;

    /// <summary>Obtém o balancete para o período indicado (datas opcionais).</summary>
    public Task<BalanceteApiResponse?> ObterBalanceteAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim    = null,
        string?   grupo      = null)
    {
        var qs = BuildQs(
            ("dataInicio", dataInicio?.ToString("yyyy-MM-dd")),
            ("dataFim",    dataFim?.ToString("yyyy-MM-dd")),
            ("grupo",      grupo));

        return _api.GetAsync<BalanceteApiResponse>($"api/balancete{qs}");
    }

    /// <summary>Obtém o balanço patrimonial para o mês/ano indicado.</summary>
    public Task<BalancoApiResponse?> ObterBalancoAsync(int? ano = null, int? mes = null)
    {
        var qs = BuildQs(
            ("ano", ano?.ToString()),
            ("mes", mes?.ToString()));

        return _api.GetAsync<BalancoApiResponse>($"api/balanco{qs}");
    }

    /// <summary>Obtém a DRE para o período indicado.</summary>
    public Task<DreSumarioApiResponse?> ObterDreAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim    = null)
    {
        var qs = BuildQs(
            ("dataInicio", dataInicio?.ToString("yyyy-MM-dd")),
            ("dataFim",    dataFim?.ToString("yyyy-MM-dd")));

        return _api.GetAsync<DreSumarioApiResponse>($"api/dre{qs}");
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static string BuildQs(params (string Key, string? Value)[] pairs)
    {
        var parts = new List<string>();
        foreach (var (key, value) in pairs)
            if (!string.IsNullOrEmpty(value))
                parts.Add($"{key}={Uri.EscapeDataString(value)}");

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}
