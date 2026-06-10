using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.DTOs.Requests;
using SmartGest.API.DTOs.Responses;
using SmartGest.API.Models;

namespace SmartGest.API.Controllers;

[ApiController]
[Route("api/contas-bancarias")]
[Authorize]
public class ContasBancariasController : ControllerBase
{
    private readonly AppDbContext _db;
    public ContasBancariasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var contas = await _db.ContasBancarias.Where(c => c.Activa).ToListAsync();
        var movMes = await _db.MovimentosBancarios
            .Where(m => m.Data.Month == DateTime.Today.Month && m.Data.Year == DateTime.Today.Year)
            .CountAsync();

        var response = new ContasBancariasSumarioResponse(
            SaldoConsolidado: contas.Sum(c => c.Moeda == "AOA" ? c.SaldoAtual : 0),
            TotalContas: contas.Count,
            MovimentosMes: movMes,
            Contas: contas.Select(Map).ToList());

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var c = await _db.ContasBancarias.FindAsync(id);
        return c is null ? NotFound() : Ok(Map(c));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Criar([FromBody] ContaBancariaRequest req)
    {
        if (await _db.ContasBancarias.AnyAsync(c => c.NIB == req.NIB))
            return Conflict(new { message = "Já existe uma conta com este NIB." });

        var conta = new ContaBancaria
        {
            Banco           = req.Banco,   NIB      = req.NIB,
            Tipo            = req.Tipo,    Moeda    = req.Moeda,
            SaldoAtual      = req.SaldoAtual, SaldoOntem = req.SaldoAtual,
            Agencia         = req.Agencia, Titular  = req.Titular,
            CorAccent       = req.CorAccent,
            ContaContabilId = req.ContaContabilId
        };

        _db.ContasBancarias.Add(conta);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obter), new { id = conta.Id }, Map(conta));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ContaBancariaRequest req)
    {
        var conta = await _db.ContasBancarias.FindAsync(id);
        if (conta is null) return NotFound();

        conta.Banco           = req.Banco;   conta.NIB       = req.NIB;
        conta.Tipo            = req.Tipo;    conta.Moeda     = req.Moeda;
        conta.SaldoAtual      = req.SaldoAtual;
        conta.Agencia         = req.Agencia; conta.Titular   = req.Titular;
        conta.CorAccent       = req.CorAccent;
        conta.ContaContabilId = req.ContaContabilId;

        await _db.SaveChangesAsync();
        return Ok(Map(conta));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var conta = await _db.ContasBancarias.FindAsync(id);
        if (conta is null) return NotFound();
        conta.Activa = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/movimentos")]
    public async Task<IActionResult> Movimentos(
        int id,
        [FromQuery] string?   tipo       = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim    = null,
        [FromQuery] string?   texto      = null)
    {
        var query = _db.MovimentosBancarios
            .Include(m => m.ContaBancaria)
            .Where(m => m.ContaBancariaId == id)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo))  query = query.Where(m => m.Tipo == tipo);
        if (dataInicio.HasValue)               query = query.Where(m => m.Data >= dataInicio.Value);
        if (dataFim.HasValue)                  query = query.Where(m => m.Data <= dataFim.Value);
        if (!string.IsNullOrWhiteSpace(texto)) query = query.Where(m =>
            m.Descricao.Contains(texto) || m.Referencia.Contains(texto));

        var items = await query
            .OrderByDescending(m => m.Data)
            .Select(m => new MovimentoBancarioResponse(
                m.Id, m.ContaBancariaId, m.ContaBancaria!.Banco,
                m.Data, m.Descricao, m.Referencia, m.Tipo, m.Valor))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("{id}/movimentos")]
    public async Task<IActionResult> CriarMovimento(int id, [FromBody] MovimentoBancarioRequest req)
    {
        if (!await _db.ContasBancarias.AnyAsync(c => c.Id == id))
            return NotFound();

        var mov = new MovimentoBancario
        {
            ContaBancariaId = id,
            Data       = req.Data,
            Descricao  = req.Descricao,
            Referencia = req.Referencia,
            Tipo       = req.Tipo,
            Valor      = req.Valor
        };

        _db.MovimentosBancarios.Add(mov);

        var conta = await _db.ContasBancarias.FindAsync(id);
        if (conta is not null)
        {
            conta.SaldoOntem = conta.SaldoAtual;
            conta.SaldoAtual += req.Tipo == "Crédito" ? req.Valor : -req.Valor;
        }

        await _db.SaveChangesAsync();
        return Ok(new MovimentoBancarioResponse(
            mov.Id, mov.ContaBancariaId, conta?.Banco ?? "",
            mov.Data, mov.Descricao, mov.Referencia, mov.Tipo, mov.Valor));
    }

    private static ContaBancariaResponse Map(ContaBancaria c)
    {
        var iniciais = string.Concat(
            c.Banco.Split(' ').Where(w => w.Length > 0).Take(2).Select(w => w[0]));
        return new(c.Id, c.Banco, c.NIB, c.Tipo, c.Moeda,
                   c.SaldoAtual, c.SaldoOntem, c.Agencia,
                   c.Titular, c.CorAccent, c.Activa, iniciais);
    }
}
