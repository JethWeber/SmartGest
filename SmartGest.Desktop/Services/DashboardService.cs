using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

public class DashboardService
{
    private readonly ApiClient _api;
    public DashboardService(ApiClient api) => _api = api;

    public Task<DashboardResponse?> ObterAsync()
        => _api.GetAsync<DashboardResponse>("api/dashboard");

    public record DashboardResponse(
        decimal TotalReceita,
        decimal TotalDespesa,
        decimal LucroLiquido,
        decimal ReceitaAnoAnterior,
        decimal DespesaAnoAnterior,
        decimal LucroAnoAnterior,
        List<FluxoMensalItem> FluxoMensal,
        List<LancamentoResumo> UltimasMovimentacoes);

    public record FluxoMensalItem(string Mes, decimal Receita, decimal Despesa, decimal Lucro);

    public record LancamentoResumo(
        int Id, DateTime Data, string Descricao, string Categoria,
        string Tipo, decimal Valor, string? ContaBancariaNome);
}
