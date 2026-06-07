using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Serviço de contas bancárias — liga o Desktop à API REST.
///
/// Endpoints cobertos:
///   GET    /api/contas-bancarias                     → sumário + lista
///   GET    /api/contas-bancarias/{id}                → conta singular
///   POST   /api/contas-bancarias                     → criar
///   PUT    /api/contas-bancarias/{id}                → actualizar
///   DELETE /api/contas-bancarias/{id}                → soft-delete
///   GET    /api/contas-bancarias/{id}/movimentos     → lista filtrada
///   POST   /api/contas-bancarias/{id}/movimentos     → criar movimento
/// </summary>
public class ContasBancariasService
{
    private readonly ApiClient _api;

    public ContasBancariasService(ApiClient api) => _api = api;

    // ── Leitura ───────────────────────────────────────────────────────────────

    /// <summary>Sumário geral com todas as contas activas.</summary>
    public Task<ContasBancariasSumarioResponse?> ListarAsync()
        => _api.GetAsync<ContasBancariasSumarioResponse>("api/contas-bancarias");

    /// <summary>Detalhe de uma conta específica.</summary>
    public Task<ContaBancariaDto?> ObterAsync(int id)
        => _api.GetAsync<ContaBancariaDto>($"api/contas-bancarias/{id}");

    /// <summary>Movimentos de uma conta, com filtros opcionais.</summary>
    public Task<List<MovimentoBancarioDto>?> ListarMovimentosAsync(
        int       contaId,
        string?   tipo       = null,
        DateTime? dataInicio = null,
        DateTime? dataFim    = null,
        string?   texto      = null)
    {
        var sb = new StringBuilder($"api/contas-bancarias/{contaId}/movimentos?");
        if (!string.IsNullOrWhiteSpace(tipo))  sb.Append($"tipo={Uri.EscapeDataString(tipo)}&");
        if (dataInicio.HasValue)               sb.Append($"dataInicio={dataInicio.Value:yyyy-MM-dd}&");
        if (dataFim.HasValue)                  sb.Append($"dataFim={dataFim.Value:yyyy-MM-dd}&");
        if (!string.IsNullOrWhiteSpace(texto)) sb.Append($"texto={Uri.EscapeDataString(texto)}&");

        return _api.GetAsync<List<MovimentoBancarioDto>>(sb.ToString().TrimEnd('?', '&'));
    }

    // ── Escrita — Contas ──────────────────────────────────────────────────────

    /// <summary>Cria uma nova conta bancária.</summary>
    public Task<ContaBancariaDto?> CriarAsync(ContaBancariaRequest req)
        => _api.PostAsync<ContaBancariaDto>("api/contas-bancarias", req);

    /// <summary>Actualiza uma conta existente.</summary>
    public Task<ContaBancariaDto?> AtualizarAsync(int id, ContaBancariaRequest req)
        => _api.PutAsync<ContaBancariaDto>($"api/contas-bancarias/{id}", req);

    /// <summary>Soft-delete de uma conta (marca Activa = false no servidor).</summary>
    public Task EliminarAsync(int id)
        => _api.DeleteAsync($"api/contas-bancarias/{id}");

    // ── Escrita — Movimentos ──────────────────────────────────────────────────

    /// <summary>Cria um movimento bancário e actualiza o saldo da conta.</summary>
    public Task<MovimentoBancarioDto?> CriarMovimentoAsync(int contaId, MovimentoBancarioRequest req)
        => _api.PostAsync<MovimentoBancarioDto>($"api/contas-bancarias/{contaId}/movimentos", req);

    // ── DTOs ──────────────────────────────────────────────────────────────────

    public record ContasBancariasSumarioResponse(
        decimal SaldoConsolidado,
        int     TotalContas,
        int     MovimentosMes,
        List<ContaBancariaDto> Contas);

    public record ContaBancariaDto(
        int     Id,
        string  Banco,
        string  NIB,
        string  Tipo,
        string  Moeda,
        decimal SaldoAtual,
        decimal SaldoOntem,
        string  Agencia,
        string  Titular,
        string  CorAccent,
        bool    Activa,
        string  Iniciais);

    public record MovimentoBancarioDto(
        int      Id,
        int      ContaBancariaId,
        string   Banco,
        DateTime Data,
        string   Descricao,
        string   Referencia,
        string   Tipo,
        decimal  Valor);

    // ── Requests ──────────────────────────────────────────────────────────────

    public record ContaBancariaRequest(
        string  Banco,
        string  NIB,
        string  Tipo,
        string  Moeda,
        decimal SaldoAtual,
        string  Agencia,
        string  Titular,
        string  CorAccent);

    public record MovimentoBancarioRequest(
        int      ContaBancariaId,
        DateTime Data,
        string   Descricao,
        string   Referencia,
        string   Tipo,
        decimal  Valor);
}
