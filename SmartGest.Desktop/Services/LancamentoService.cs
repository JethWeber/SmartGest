using System;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Serviço de lançamentos financeiros.
/// Endpoints usados:
///   POST /api/lancamentos   → criar lançamento
///   GET  /api/lancamentos   → listar (para uso futuro)
/// </summary>
public class LancamentoService
{
    private readonly ApiClient _api;

    public LancamentoService(ApiClient api) => _api = api;

    /// <summary>
    /// Cria um novo lançamento na API.
    /// Devolve o lançamento criado com o Id atribuído pelo servidor.
    /// </summary>
    public async Task<LancamentoResponse> CriarAsync(LancamentoRequest req)
    {
        var resp = await _api.PostAsync<LancamentoResponse>("api/lancamentos", req);
        return resp ?? throw new InvalidOperationException("Servidor não devolveu o lançamento criado.");
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    /// <summary>Corpo do pedido POST /api/lancamentos (espelha LancamentoRequest da API).</summary>
    public record LancamentoRequest(
        DateTime Data,
        string   Descricao,
        string   Categoria,
        string   Tipo,               // "Entrada" | "Saída"
        decimal  Valor,
        string   Beneficiario,
        string   MetodoPagamento,
        string   CaminhoDocumento,
        string   Observacoes,
        string   CentroCusto,
        string   ReferenciaInterna,
        int?     ContaBancariaId);

    /// <summary>Corpo da resposta 201 Created (espelha LancamentoResponse da API).</summary>
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
}
