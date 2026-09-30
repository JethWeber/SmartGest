using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public class DashboardService
{
    private readonly SmartGestDbContext _db;
    public DashboardService(SmartGestDbContext db) => _db = db;

    public async Task<DashboardResponse?> ObterAsync()
    {
        var hoje = DateTime.Today;
        var inicioAno = new DateTime(hoje.Year, 1, 1);
        var lanc = await _db.Lancamentos.AsNoTracking().Where(x => !x.Anulado && x.Data >= inicioAno).ToListAsync();
        var anterior = await _db.Lancamentos.AsNoTracking().Where(x => !x.Anulado && x.Data >= inicioAno.AddYears(-1) && x.Data < inicioAno).ToListAsync();

        decimal rec = lanc.Where(x => x.Tipo=="Entrada").Sum(x=>x.Valor);
        decimal desp = lanc.Where(x => x.Tipo=="Saída").Sum(x=>x.Valor);
        decimal recAnt = anterior.Where(x => x.Tipo=="Entrada").Sum(x=>x.Valor);
        decimal despAnt = anterior.Where(x => x.Tipo=="Saída").Sum(x => x.Valor);

        var fluxo = Enumerable.Range(1,12).Select(m => {
            var a=lanc.Where(x=>x.Data.Month==m);
            var r=a.Where(x=>x.Tipo=="Entrada").Sum(x=>x.Valor);
            var d=a.Where(x=>x.Tipo=="Saída").Sum(x=>x.Valor);
            return new FluxoMensalItem(new DateTime(hoje.Year,m,1).ToString("MMM"),r,d,r-d);
        }).ToList();

        var ult = await _db.Lancamentos.AsNoTracking().Where(x=>!x.Anulado)
            .OrderByDescending(x=>x.Data).ThenByDescending(x=>x.Id).Take(10).ToListAsync();

        var contaIds=ult.Where(x=>x.ContaBancariaId.HasValue).Select(x=>x.ContaBancariaId!.Value).Distinct().ToList();
        var contas=await _db.ContasBancarias.AsNoTracking().Where(x=>contaIds.Contains(x.Id))
            .ToDictionaryAsync(x=>x.Id,x=>x.Banco);

        return new(rec,desp,rec-desp,recAnt,despAnt,recAnt-despAnt,fluxo,
            ult.Select(x=>new LancamentoResumo(x.Id,x.Data,x.Descricao,x.Categoria,x.Tipo,x.Valor,
                x.ContaBancariaId.HasValue && contas.TryGetValue(x.ContaBancariaId.Value,out var n)?n:null)).ToList());
    }

    public record DashboardResponse(decimal TotalReceita, decimal TotalDespesa, decimal LucroLiquido,
        decimal ReceitaAnoAnterior, decimal DespesaAnoAnterior, decimal LucroAnoAnterior,
        List<FluxoMensalItem> FluxoMensal, List<LancamentoResumo> UltimasMovimentacoes);
    public record FluxoMensalItem(string Mes, decimal Receita, decimal Despesa, decimal Lucro);
    public record LancamentoResumo(int Id, DateTime Data, string Descricao, string Categoria, string Tipo, decimal Valor, string? ContaBancariaNome);
}