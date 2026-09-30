using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartGest.Core.Domain;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public class ContasBancariasService
{
    private readonly SmartGestDbContext _db;
    public ContasBancariasService(SmartGestDbContext db) => _db = db;

    public async Task<ContasBancariasSumarioResponse?> ListarAsync()
    {
        var contas = await _db.ContasBancarias.AsNoTracking().Where(x=>x.Activa).OrderBy(x=>x.Id).ToListAsync();
        var mes = new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
        var movMes = await _db.MovimentosBancarios.AsNoTracking().CountAsync(x=>x.Data>=mes);
        return new(contas.Sum(x=>x.SaldoAtual),contas.Count,movMes,contas.Select(Map).ToList());
    }

    public async Task<ContaBancariaDto?> ObterAsync(int id)
    {
        var x=await _db.ContasBancarias.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);
        return x is null?null:Map(x);
    }

    public async Task<List<MovimentoBancarioDto>?> ListarMovimentosAsync(int contaId,string? tipo=null,DateTime? dataInicio=null,DateTime? dataFim=null,string? texto=null)
    {
        var q=_db.MovimentosBancarios.AsNoTracking().Where(x=>x.ContaBancariaId==contaId);
        if(!string.IsNullOrWhiteSpace(tipo)) q=q.Where(x=>x.Tipo==tipo);
        if(dataInicio.HasValue) q=q.Where(x=>x.Data>=dataInicio.Value.Date);
        if(dataFim.HasValue) q=q.Where(x=>x.Data<dataFim.Value.Date.AddDays(1));
        if(!string.IsNullOrWhiteSpace(texto)) { var t=texto.ToLower(); q=q.Where(x=>x.Descricao.ToLower().Contains(t)||x.Referencia.ToLower().Contains(t)); }
        var conta=await _db.ContasBancarias.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==contaId);
        return await q.OrderByDescending(x=>x.Data).Select(x=>new MovimentoBancarioDto(x.Id,x.ContaBancariaId,conta==null?"":conta.Banco,x.Data,x.Descricao,x.Referencia,x.Tipo,x.Valor)).ToListAsync();
    }

    public async Task<ContaBancariaDto?> CriarAsync(ContaBancariaRequest req)
    {
        if(await _db.ContasBancarias.AnyAsync(x=>x.NIB==req.NIB && x.Activa))
            throw new InvalidOperationException("Já existe uma conta com este NIB.");
        var x=new ContaBancaria(req.Banco,req.NIB,req.Tipo,req.Moeda);
        _db.ContasBancarias.Add(x);
        await _db.SaveChangesAsync();
        if(req.SaldoAtual!=0 || !string.IsNullOrWhiteSpace(req.Agencia) || !string.IsNullOrWhiteSpace(req.Titular) || !string.IsNullOrWhiteSpace(req.CorAccent))
            await _db.Database.ExecuteSqlInterpolatedAsync($"UPDATE ContasBancarias SET SaldoAtual={req.SaldoAtual}, Agencia={req.Agencia}, Titular={req.Titular}, CorAccent={req.CorAccent} WHERE Id={x.Id}");
        return await ObterAsync(x.Id);
    }

    public async Task<ContaBancariaDto?> AtualizarAsync(int id, ContaBancariaRequest req)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($"UPDATE ContasBancarias SET Banco={req.Banco}, NIB={req.NIB}, Tipo={req.Tipo}, Moeda={req.Moeda}, SaldoAtual={req.SaldoAtual}, Agencia={req.Agencia}, Titular={req.Titular}, CorAccent={req.CorAccent} WHERE Id={id}");
        return await ObterAsync(id);
    }

    public async Task EliminarAsync(int id)
        => await _db.Database.ExecuteSqlInterpolatedAsync($"UPDATE ContasBancarias SET Activa=0 WHERE Id={id}");

    public async Task<MovimentoBancarioDto?> CriarMovimentoAsync(int contaId, MovimentoBancarioRequest req)
    {
        var conta=await _db.ContasBancarias.FirstOrDefaultAsync(x=>x.Id==contaId && x.Activa)
            ?? throw new InvalidOperationException("Conta bancária não encontrada.");
        var x=new MovimentoBancario(contaId,req.Data,req.Descricao,req.Referencia,req.Tipo,req.Valor);
        _db.MovimentosBancarios.Add(x);
        var novoSaldo = req.Tipo=="Crédito" ? conta.SaldoAtual+req.Valor : conta.SaldoAtual-req.Valor;
        await _db.SaveChangesAsync();
        await _db.Database.ExecuteSqlInterpolatedAsync($"UPDATE ContasBancarias SET SaldoOntem=SaldoAtual, SaldoAtual={novoSaldo} WHERE Id={contaId}");
        return new(x.Id,contaId,conta.Banco,x.Data,x.Descricao,x.Referencia,x.Tipo,x.Valor);
    }

    private static ContaBancariaDto Map(ContaBancaria x)
    {
        var iniciais=string.Concat(x.Banco.Split(' ',StringSplitOptions.RemoveEmptyEntries).Take(2).Select(s=>char.ToUpperInvariant(s[0])));
        return new(x.Id,x.Banco,x.NIB,x.Tipo,x.Moeda,x.SaldoAtual,x.SaldoOntem,x.Agencia,x.Titular,x.CorAccent,x.Activa,iniciais);
    }

    public record ContasBancariasSumarioResponse(decimal SaldoConsolidado,int TotalContas,int MovimentosMes,List<ContaBancariaDto> Contas);
    public record ContaBancariaDto(int Id,string Banco,string NIB,string Tipo,string Moeda,decimal SaldoAtual,decimal SaldoOntem,string Agencia,string Titular,string CorAccent,bool Activa,string Iniciais);
    public record MovimentoBancarioDto(int Id,int ContaBancariaId,string Banco,DateTime Data,string Descricao,string Referencia,string Tipo,decimal Valor);
    public record ContaBancariaRequest(string Banco,string NIB,string Tipo,string Moeda,decimal SaldoAtual,string Agencia,string Titular,string CorAccent);
    public record MovimentoBancarioRequest(int ContaBancariaId,DateTime Data,string Descricao,string Referencia,string Tipo,decimal Valor);
}