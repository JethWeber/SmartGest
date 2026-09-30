using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartGest.Application.Lancamentos;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public class LancamentoService
{
    private readonly IDbContextFactory<SmartGestDbContext> _factory;

    public LancamentoService(IDbContextFactory<SmartGestDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<LancamentoResponse> CriarAsync(LancamentoRequest req)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var categoria = await db.CategoriaContabeis.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.CategoriaId && x.Ativo);
        if (categoria is null || categoria.Tipo != req.Tipo)
            throw new InvalidOperationException("Categoria financeira inválida para o tipo de lançamento.");

        var entity = new SmartGest.Core.Domain.Lancamento(
            req.Data, req.Descricao, req.Tipo, req.Valor, req.CategoriaId,
            req.Beneficiario, req.MetodoPagamento, req.CaminhoDocumento,
            req.Observacoes, req.CentroCusto, req.ReferenciaInterna, req.ContaBancariaId);
        entity.DefinirCategoria(categoria.Nome);
        db.Lancamentos.Add(entity);
        await db.SaveChangesAsync();
        var categoria = await db.CategoriaContabeis.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == entity.CategoriaContabilId);
        return Map(entity, categoria?.Nome, null);
    }

    public async Task<LancamentosPageResponse> ListarAsync(
        string? tipo = null, DateTime? dataInicio = null, DateTime? dataFim = null,
        string? texto = null, int? contaId = null, bool incluirAnulados = false,
        int pagina = 1, int tamPagina = 50)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var q = db.Lancamentos.AsNoTracking().AsQueryable();
        if (!incluirAnulados) q = q.Where(x => !x.Anulado);
        if (!string.IsNullOrWhiteSpace(tipo)) q = q.Where(x => x.Tipo == tipo);
        if (dataInicio.HasValue) q = q.Where(x => x.Data >= dataInicio.Value.Date);
        if (dataFim.HasValue) q = q.Where(x => x.Data < dataFim.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim().ToLower();
            q = q.Where(x => x.Descricao.ToLower().Contains(t) || x.Categoria.ToLower().Contains(t) || x.Beneficiario.ToLower().Contains(t));
        }
        if (contaId.HasValue) q = q.Where(x => x.ContaBancariaId == contaId);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(x => x.Data).ThenByDescending(x => x.Id)
            .Skip((pagina - 1) * tamPagina).Take(tamPagina).ToListAsync();

        var contaIds = items.Where(x => x.ContaBancariaId.HasValue).Select(x => x.ContaBancariaId!.Value).Distinct().ToList();
        var contas = await db.ContasBancarias.AsNoTracking().Where(x => contaIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => $"{x.Banco} · {x.SaldoAtual:N0} {x.Moeda}");

        return new(total, pagina, tamPagina, items.Select(x =>
            Map(x, x.Categoria, x.ContaBancariaId.HasValue && contas.TryGetValue(x.ContaBancariaId.Value, out var n) ? n : null)).ToList());
    }

    private static LancamentoResponse Map(SmartGest.Core.Domain.Lancamento x, string? categoria, string? conta) =>
        new(x.Id, x.Data, x.Descricao, categoria ?? x.Categoria, x.Tipo, x.Valor,
            x.Beneficiario, x.MetodoPagamento, x.CaminhoDocumento, x.Observacoes,
            x.CentroCusto, x.ReferenciaInterna, x.CriadoEm, x.ContaBancariaId, conta);

    public record LancamentoRequest(
        DateTime Data, string Descricao, string Tipo, decimal Valor, int CategoriaId,
        string? Beneficiario, string? MetodoPagamento, string? CaminhoDocumento,
        string? Observacoes, string? CentroCusto, string? ReferenciaInterna, int? ContaBancariaId);

    public record LancamentoResponse(
        int Id, DateTime Data, string Descricao, string Categoria, string Tipo, decimal Valor,
        string Beneficiario, string MetodoPagamento, string CaminhoDocumento, string Observacoes,
        string CentroCusto, string ReferenciaInterna, DateTime CriadoEm, int? ContaBancariaId,
        string? ContaBancariaNome);

    public record LancamentosPageResponse(int Total, int Pagina, int TamPagina, List<LancamentoResponse> Items);
}