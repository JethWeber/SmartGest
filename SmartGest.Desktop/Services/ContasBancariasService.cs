using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Serviço de contas bancárias.
/// Endpoints usados:
///   GET  /api/contas-bancarias          → sumário + lista de contas
///   GET  /api/contas-bancarias/{id}/movimentos
///   POST /api/contas-bancarias/{id}/movimentos
/// </summary>
public class ContasBancariasService
{
    private readonly ApiClient _api;

    public ContasBancariasService(ApiClient api) => _api = api;

    /// <summary>
    /// Devolve o sumário com todas as contas activas.
    /// Usado pelo dropdown de "Conta de Origem" no NovoLancamentoViewModel
    /// e pela página ContaseBancosViewModel.
    /// </summary>
    public Task<ContasBancariasSumarioResponse?> ListarAsync()
        => _api.GetAsync<ContasBancariasSumarioResponse>("api/contas-bancarias");

    // ── DTOs de resposta ──────────────────────────────────────────────────────

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
}
