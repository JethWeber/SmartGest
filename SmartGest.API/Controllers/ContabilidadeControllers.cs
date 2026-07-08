using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartGest.API.Services;
using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.DTOs.Requests;
using SmartGest.API.DTOs.Responses;
using SmartGest.API.Models;
namespace SmartGest.API.Controllers;

// ── PLANO DE CONTAS ───────────────────────────────────────────────────────────

[ApiController]
[Route("api/contas-contabeis")]
[Authorize]
public class ContasContabeisController : ControllerBase
{
    private readonly AppDbContext _db;
    public ContasContabeisController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? grupo = null)
    {
        var query = _db.ContasContabeis.AsQueryable();
        if (!string.IsNullOrWhiteSpace(grupo))
            query = query.Where(c => c.Grupo == grupo);

        var items = await query
            .OrderBy(c => c.Codigo)
            .Select(c => new ContaContabilResponse(c.Id, c.Codigo, c.Nome, c.Grupo, c.IsDevedora, c.Activa, c.Corrente))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var c = await _db.ContasContabeis.FindAsync(id);
        return c is null ? NotFound()
            : Ok(new ContaContabilResponse(c.Id, c.Codigo, c.Nome, c.Grupo, c.IsDevedora, c.Activa, c.Corrente));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Criar([FromBody] ContaContabilRequest req)
    {
        if (await _db.ContasContabeis.AnyAsync(c => c.Codigo == req.Codigo))
            return Conflict(new { message = "Código de conta já existe." });

        if (req.Grupo is "Ativo" or "Passivo" && req.Corrente is null)
            return BadRequest(new { message = "Contas de Ativo ou Passivo precisam indicar se são Corrente ou Não Corrente." });

        var conta = new ContaContabil
            { Codigo = req.Codigo, Nome = req.Nome, Grupo = req.Grupo, IsDevedora = req.IsDevedora, Corrente = req.Corrente };
        _db.ContasContabeis.Add(conta);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obter), new { id = conta.Id },
            new ContaContabilResponse(conta.Id, conta.Codigo, conta.Nome, conta.Grupo, conta.IsDevedora, conta.Activa, conta.Corrente));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ContaContabilRequest req)
    {
        var c = await _db.ContasContabeis.FindAsync(id);
        if (c is null) return NotFound();

        if (req.Grupo is "Ativo" or "Passivo" && req.Corrente is null)
            return BadRequest(new { message = "Contas de Ativo ou Passivo precisam indicar se são Corrente ou Não Corrente." });

        c.Codigo = req.Codigo; c.Nome = req.Nome; c.Grupo = req.Grupo; c.IsDevedora = req.IsDevedora; c.Corrente = req.Corrente;
        await _db.SaveChangesAsync();
        return Ok(new ContaContabilResponse(c.Id, c.Codigo, c.Nome, c.Grupo, c.IsDevedora, c.Activa, c.Corrente));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var c = await _db.ContasContabeis.FindAsync(id);
        if (c is null) return NotFound();
        c.Activa = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ── BALANCETE ─────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/balancete")]
[Authorize]
public class BalanceteController : ControllerBase
{
    private readonly RelatoriosService _relatorios;
    public BalanceteController(RelatoriosService relatorios) => _relatorios = relatorios;

    [HttpGet]
    public async Task<IActionResult> Obter(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim    = null,
        [FromQuery] string?   grupo      = null)
        => Ok(await _relatorios.ObterBalanceteAsync(dataInicio, dataFim, grupo));
}

// ── BALANÇO PATRIMONIAL ───────────────────────────────────────────────────────

[ApiController]
[Route("api/balanco")]
[Authorize]
public class BalancoController : ControllerBase
{
    private readonly RelatoriosService _relatorios;
    public BalancoController(RelatoriosService relatorios) => _relatorios = relatorios;

    [HttpGet]
    public async Task<IActionResult> Obter(
        [FromQuery] int? ano = null,
        [FromQuery] int? mes = null)
        => Ok(await _relatorios.ObterBalancoAsync(ano, mes));
}

// ── DRE ───────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/dre")]
[Authorize]
public class DreController : ControllerBase
{
    private readonly RelatoriosService _relatorios;
    public DreController(RelatoriosService relatorios) => _relatorios = relatorios;

    [HttpGet]
    public async Task<IActionResult> Obter(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim    = null)
        => Ok(await _relatorios.ObterDreAsync(dataInicio, dataFim));
}

// ── FLUXO DE CAIXA ────────────────────────────────────────────────────────────

[ApiController]
[Route("api/fluxo-caixa")]
[Authorize]
public class FluxoCaixaController : ControllerBase
{
    private readonly RelatoriosService _relatorios;
    public FluxoCaixaController(RelatoriosService relatorios) => _relatorios = relatorios;

    /// <summary>
    /// Retorna o fluxo de caixa do período, agrupado por tipo de operação.
    /// GET /api/fluxo-caixa?dataInicio=2025-01-01&amp;dataFim=2025-12-31
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Obter(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim    = null)
        => Ok(await _relatorios.ObterFluxoCaixaAsync(dataInicio, dataFim));
}