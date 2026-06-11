using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Serviço de lançamentos financeiros.
/// Endpoints usados:
///   POST /api/lancamentos   → criar lançamento
///   GET  /api/lancamentos   → listar com filtros e paginação
/// </summary>
public class LancamentoService
{
    private readonly ApiClient _api;
    public LancamentoService(ApiClient api) => _api = api;

    /// <summary>Cria um novo lançamento na API.</summary>
    public async Task<LancamentoResponse> CriarAsync(LancamentoRequest req)
    {
        var resp = await _api.PostAsync<LancamentoResponse>("api/lancamentos", req);
        return resp ?? throw new InvalidOperationException("Servidor não devolveu o lançamento criado.");
    }

    /// <summary>Lista lançamentos com filtros opcionais.</summary>
    public async Task<LancamentosPageResponse> ListarAsync(
        string?   tipo            = null,
        DateTime? dataInicio      = null,
        DateTime? dataFim         = null,
        string?   texto           = null,
        int?      contaId         = null,
        bool      incluirAnulados = false,
        int       pagina          = 1,
        int       tamPagina       = 50)
    {
        var qs = $"api/lancamentos?pagina={pagina}&tamPagina={tamPagina}&incluirAnulados={incluirAnulados}";
        if (!string.IsNullOrWhiteSpace(tipo))  qs += $"&tipo={Uri.EscapeDataString(tipo)}";
        if (!string.IsNullOrWhiteSpace(texto)) qs += $"&texto={Uri.EscapeDataString(texto)}";
        if (dataInicio.HasValue) qs += $"&dataInicio={dataInicio.Value:yyyy-MM-dd}";
        if (dataFim.HasValue)    qs += $"&dataFim={dataFim.Value:yyyy-MM-dd}";
        if (contaId.HasValue)    qs += $"&contaBancariaId={contaId.Value}";

        var resp = await _api.GetAsync<LancamentosPageResponse>(qs);
        return resp ?? new LancamentosPageResponse(0, pagina, tamPagina, new());
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    public record LancamentoRequest(
        DateTime Data,
        string   Descricao,
        string   Tipo,
        decimal  Valor,
        int      CategoriaId,
        string?  Beneficiario,
        string?  MetodoPagamento,
        string?  CaminhoDocumento,
        string?  Observacoes,
        string?  CentroCusto,
        string?  ReferenciaInterna,
        int?     ContaBancariaId);

    public record LancamentoResponse(
        int      Id,
        DateTime Data,
        string   Descricao,
        string   Categoria,
        string   Tipo,
        decimal  Valor,
        string   Beneficiario,
        string   MetodoPagamento,
        string   CaminhoDocumento,
        string   Observacoes,
        string   CentroCusto,
        string   ReferenciaInterna,
        DateTime CriadoEm,
        int?     ContaBancariaId,
        string?  ContaBancariaNome);

    public record LancamentosPageResponse(
        int                    Total,
        int                    Pagina,
        int                    TamPagina,
        List<LancamentoResponse> Items);
}