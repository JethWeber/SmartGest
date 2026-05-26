using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.DTOs.Requests;
using SmartGest.API.DTOs.Responses;
using SmartGest.API.Models;

namespace SmartGest.API.Controllers;

[ApiController]
[Route("api/lancamentos")]
[Authorize]
public class LancamentosController : ControllerBase
{
    private readonly AppDbContext _db;
    public LancamentosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string?   tipo       = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim    = null,
        [FromQuery] string?   texto      = null,
        [FromQuery] int?      contaId    = null,
        [FromQuery] int       pagina     = 1,
        [FromQuery] int       tamPagina  = 50)
    {
        var query = _db.Lancamentos.Include(l => l.ContaBancaria).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo))       query = query.Where(l => l.Tipo == tipo);
        if (dataInicio.HasValue)                     query = query.Where(l => l.Data >= dataInicio.Value);
        if (dataFim.HasValue)                        query = query.Where(l => l.Data <= dataFim.Value);
        if (!string.IsNullOrWhiteSpace(texto))       query = query.Where(l =>
            l.Descricao.Contains(texto) || l.Categoria.Contains(texto) || l.Beneficiario.Contains(texto));
        if (contaId.HasValue)                        query = query.Where(l => l.ContaBancariaId == contaId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.Data)
            .Skip((pagina - 1) * tamPagina)
            .Take(tamPagina)
            .Select(l => Map(l))
            .ToListAsync();

        return Ok(new { total, pagina, tamPagina, items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var l = await _db.Lancamentos
            .Include(x => x.ContaBancaria)
            .FirstOrDefaultAsync(x => x.Id == id);
        return l is null ? NotFound() : Ok(Map(l));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] LancamentoRequest req)
    {
        var lanc = new Lancamento
        {
            Data              = req.Data,
            Descricao         = req.Descricao,
            Categoria         = req.Categoria,
            Tipo              = req.Tipo,
            Valor             = req.Valor,
            Beneficiario      = req.Beneficiario,
            MetodoPagamento   = req.MetodoPagamento,
            CaminhoDocumento  = req.CaminhoDocumento,
            Observacoes       = req.Observacoes,
            CentroCusto       = req.CentroCusto,
            ReferenciaInterna = req.ReferenciaInterna,
            ContaBancariaId   = req.ContaBancariaId,
        };

        _db.Lancamentos.Add(lanc);
        await _db.SaveChangesAsync();

        if (req.ContaBancariaId.HasValue)
        {
            var conta = await _db.ContasBancarias.FindAsync(req.ContaBancariaId.Value);
            if (conta is not null)
            {
                conta.SaldoOntem = conta.SaldoAtual;
                conta.SaldoAtual += req.Tipo == "Entrada" ? req.Valor : -req.Valor;
                await _db.SaveChangesAsync();
            }
        }

        return CreatedAtAction(nameof(Obter), new { id = lanc.Id }, Map(lanc));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] LancamentoRequest req)
    {
        var lanc = await _db.Lancamentos.FindAsync(id);
        if (lanc is null) return NotFound();

        lanc.Data              = req.Data;
        lanc.Descricao         = req.Descricao;
        lanc.Categoria         = req.Categoria;
        lanc.Tipo              = req.Tipo;
        lanc.Valor             = req.Valor;
        lanc.Beneficiario      = req.Beneficiario;
        lanc.MetodoPagamento   = req.MetodoPagamento;
        lanc.CaminhoDocumento  = req.CaminhoDocumento;
        lanc.Observacoes       = req.Observacoes;
        lanc.CentroCusto       = req.CentroCusto;
        lanc.ReferenciaInterna = req.ReferenciaInterna;
        lanc.ContaBancariaId   = req.ContaBancariaId;

        await _db.SaveChangesAsync();
        return Ok(Map(lanc));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var lanc = await _db.Lancamentos.FindAsync(id);
        if (lanc is null) return NotFound();
        _db.Lancamentos.Remove(lanc);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static LancamentoResponse Map(Lancamento l) => new(
        l.Id, l.Data, l.Descricao, l.Categoria, l.Tipo, l.Valor,
        l.Beneficiario, l.MetodoPagamento, l.CaminhoDocumento,
        l.Observacoes, l.CentroCusto, l.ReferenciaInterna, l.CriadoEm,
        l.ContaBancariaId, l.ContaBancaria?.Banco);
}
