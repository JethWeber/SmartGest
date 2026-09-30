using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public record BalanceteItemResponse(string Codigo,string Nome,string Grupo,double SaldoAnteriorDebito,double SaldoAnteriorCredito,double MovDebito,double MovCredito,double SaldoFinalDebito,double SaldoFinalCredito);
public record BalanceteApiResponse(BalancetePeriodo Periodo,double TotalDebitos,double TotalCreditos,List<BalanceteItemResponse> Items);
public record BalancetePeriodo(DateTime Inicio,DateTime Fim);
public record BalancoLinhaResponse(string Descricao,decimal Valor,bool IsDeducao=false);
public record BalancoApiResponse(List<BalancoLinhaResponse> AtivoCorrentes,List<BalancoLinhaResponse> AtivoNaoCorrentes,List<BalancoLinhaResponse> PassivosCorrentes,List<BalancoLinhaResponse> CapitalProprio,decimal TotalAtivo,decimal TotalPassivo,decimal TotalCapitalProprio,decimal TotalPassivoMaisCapital);
public record DreItemResponse(string Codigo,string Descricao,string Grupo,decimal ValorOrcado,decimal ValorRealizado,bool IsReceita,DateTime DataOrigem);
public record DreFluxoMensalItem(string Mes,decimal Receita,decimal Despesa,decimal Lucro);
public record DreSumarioApiResponse(decimal TotalReceitas,decimal TotalCustos,decimal ResultadoLiquido,List<DreItemResponse> Linhas,List<DreFluxoMensalItem> FluxoMensal);

public class ContabilidadeService
{
    private readonly SmartGestDbContext _db;
    public ContabilidadeService(SmartGestDbContext db)=>_db=db;

    public async Task<BalanceteApiResponse?> ObterBalanceteAsync(DateTime? dataInicio=null,DateTime? dataFim=null,string? grupo=null)
    {
        var inicio=dataInicio?.Date ?? new DateTime(DateTime.Today.Year,1,1);
        var fim=(dataFim?.Date ?? DateTime.Today).AddDays(1);
        var cats=await _db.CategoriaContabeis.AsNoTracking().ToListAsync();
        var lanc=await _db.Lancamentos.AsNoTracking().Where(x=>!x.Anulado && x.Data>=inicio && x.Data<fim).ToListAsync();
        var rows=new Dictionary<string,(string Nome,string Grupo,double Deb,double Cred)>();
        void Add(string code,string nome,string g,double deb,double cred)
        {
            if(rows.TryGetValue(code,out var v)) rows[code]=(v.Nome,v.Grupo,v.Deb+deb,v.Cred+cred);
            else rows[code]=(nome,g,deb,cred);
        }
        foreach(var l in lanc){
            var c=cats.FirstOrDefault(x=>x.Id==l.CategoriaContabilId);
            if(c is null) continue;
            var g=l.Tipo=="Entrada"?"Receita":"Despesa";
            Add(c.ContaDebito,c.ContaDebito,g,(double)l.Valor,0);
            Add(c.ContaCredito,c.ContaCredito,g,0,(double)l.Valor);
        }
        var items=rows.Select(k=>new BalanceteItemResponse(k.Key,k.Value.Nome,k.Value.Grupo,0,0,k.Value.Deb,k.Value.Cred,Math.Max(0,k.Value.Deb-k.Value.Cred),Math.Max(0,k.Value.Cred-k.Value.Deb))).ToList();
        if(!string.IsNullOrWhiteSpace(grupo)) items=items.Where(x=>x.Grupo==grupo).ToList();
        return new(new(inicio,fim.AddDays(-1)),items.Sum(x=>x.MovDebito),items.Sum(x=>x.MovCredito),items);
    }

    public async Task<BalancoApiResponse?> ObterBalancoAsync(int? ano=null,int? mes=null)
    {
        var y=ano??DateTime.Today.Year; var m=mes??DateTime.Today.Month;
        var inicio=new DateTime(y,m,1); var fim=inicio.AddMonths(1);
        var cats=await _db.CategoriaContabeis.AsNoTracking().ToListAsync();
        var lanc=await _db.Lancamentos.AsNoTracking().Where(x=>!x.Anulado && x.Data<fim).ToListAsync();
        var dict=new Dictionary<string,decimal>();
        foreach(var l in lanc){var c=cats.FirstOrDefault(x=>x.Id==l.CategoriaContabilId); if(c is null||string.IsNullOrWhiteSpace(c.GrupoBalanco)) continue; var sign=l.Tipo=="Entrada"?1m:-1m; dict[c.GrupoBalanco]=dict.GetValueOrDefault(c.GrupoBalanco)+l.Valor*sign;}
        var ativoC=Line(dict,"AtivoCorrente"); var ativoNC=Line(dict,"AtivoNaoCorrente"); var passC=Line(dict,"PassivoCorrente"); var passNC=Line(dict,"PassivoNaoCorrente"); var cap=Line(dict,"CapitalProprio");
        var ta=ativoC.Sum(x=>x.Valor)+ativoNC.Sum(x=>x.Valor); var tp=passC.Sum(x=>x.Valor)+passNC.Sum(x=>x.Valor); var tc=cap.Sum(x=>x.Valor);
        return new(ativoC,ativoNC,passC,passNC,cap,ta,tp,tc,tp+tc);
    }

    public async Task<DreSumarioApiResponse?> ObterDreAsync(DateTime? dataInicio=null,DateTime? dataFim=null)
    {
        var inicio=dataInicio?.Date??new DateTime(DateTime.Today.Year,1,1); var fim=(dataFim?.Date??DateTime.Today).AddDays(1);
        var cats=await _db.CategoriaContabeis.AsNoTracking().ToListAsync();
        var lanc=await _db.Lancamentos.AsNoTracking().Where(x=>!x.Anulado&&x.Data>=inicio&&x.Data<fim).ToListAsync();
        var linhas=lanc.GroupBy(x=>x.CategoriaContabilId).Select(g=>{var l=g.First();var c=cats.FirstOrDefault(x=>x.Id==g.Key);var val=g.Sum(x=>x.Valor);var receita=l.Tipo=="Entrada";return new DreItemResponse(c?.ContaCredito??"",c?.Nome??l.Categoria,c?.GrupoDre??"",0,val,receita,l.Data);}).ToList();
        var rec=lanc.Where(x=>x.Tipo=="Entrada").Sum(x=>x.Valor); var desp=lanc.Where(x=>x.Tipo=="Saída").Sum(x=>x.Valor);
        var fluxo=Enumerable.Range(1,12).Select(m=>{var a=lanc.Where(x=>x.Data.Month==m);var r=a.Where(x=>x.Tipo=="Entrada").Sum(x=>x.Valor);var d=a.Where(x=>x.Tipo=="Saída").Sum(x=>x.Valor);return new DreFluxoMensalItem(new DateTime(inicio.Year,m,1).ToString("MMM"),r,d,r-d);}).ToList();
        return new(rec,desp,rec-desp,linhas,fluxo);
    }

    private static List<BalancoLinhaResponse> Line(Dictionary<string,decimal> d,string key)
        => d.TryGetValue(key,out var v)&&v!=0?new(){new(key,v)}:new();
}