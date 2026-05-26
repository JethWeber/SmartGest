using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.DTOs.Requests;
using SmartGest.API.DTOs.Responses;
using SmartGest.API.Models;

namespace SmartGest.API.Controllers;

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
            .Select(c => new ContaContabilResponse(c.Id, c.Codigo, c.Nome, c.Grupo, c.IsDevedora, c.Activa))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var c = await _db.ContasContabeis.FindAsync(id);
        return c is null ? NotFound()
            : Ok(new ContaContabilResponse(c.Id, c.Codigo, c.Nome, c.Grupo, c.IsDevedora, c.Activa));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Criar([FromBody] ContaContabilRequest req)
    {
        if (await _db.ContasContabeis.AnyAsync(c => c.Codigo == req.Codigo))
            return Conflict(new { message = "Código de conta já existe." });

        var conta = new ContaContabil
            { Codigo = req.Codigo, Nome = req.Nome, Grupo = req.Grupo, IsDevedora = req.IsDevedora };
        _db.ContasContabeis.Add(conta);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obter), new { id = conta.Id },
            new ContaContabilResponse(conta.Id, conta.Codigo, conta.Nome, conta.Grupo, conta.IsDevedora, conta.Activa));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ContaContabilRequest req)
    {
        var c = await _db.ContasContabeis.FindAsync(id);
        if (c is null) return NotFound();
        c.Codigo = req.Codigo; c.Nome = req.Nome; c.Grupo = req.Grupo; c.IsDevedora = req.IsDevedora;
        await _db.SaveChangesAsync();
        return Ok(new ContaContabilResponse(c.Id, c.Codigo, c.Nome, c.Grupo, c.IsDevedora, c.Activa));
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
    private readonly AppDbContext _db;
    public BalanceteController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Obter(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim    = null,
        [FromQuery] string?   grupo      = null)
    {
        var inicio = dataInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var fim    = dataFim    ?? DateTime.Today;

        var movimentos = await _db.LancamentoDetalhes
            .Include(d => d.Lancamento)
            .Include(d => d.ContaContabil)
            .Where(d => d.Lancamento!.Data >= inicio && d.Lancamento.Data <= fim)
            .ToListAsync();

        var contas = await _db.ContasContabeis
            .Where(c => c.Activa && (grupo == null || c.Grupo == grupo))
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        var items = contas.Select(conta =>
        {
            var movConta = movimentos.Where(d => d.ContaContabilId == conta.Id).ToList();
            var movDeb   = movConta.Sum(d => d.Debito);
            var movCre   = movConta.Sum(d => d.Credito);

            var anteriores = _db.LancamentoDetalhes
                .Include(d => d.Lancamento)
                .Where(d => d.ContaContabilId == conta.Id && d.Lancamento!.Data < inicio)
                .ToList();

            var saldoAntDeb = conta.IsDevedora
                ? Math.Max(0, anteriores.Sum(d => d.Debito - d.Credito)) : 0;
            var saldoAntCre = !conta.IsDevedora
                ? Math.Max(0, anteriores.Sum(d => d.Credito - d.Debito)) : 0;

            var saldoFinalDeb = Math.Max(0, saldoAntDeb + movDeb - saldoAntCre - movCre);
            var saldoFinalCre = Math.Max(0, saldoAntCre + movCre - saldoAntDeb - movDeb);

            return new BalanceteItemResponse(
                conta.Codigo, conta.Nome, conta.Grupo,
                saldoAntDeb, saldoAntCre, movDeb, movCre,
                saldoFinalDeb, saldoFinalCre);
        }).ToList();

        return Ok(new
        {
            periodo       = new { inicio, fim },
            totalDebitos  = items.Sum(i => i.MovDebito + i.SaldoAnteriorDebito),
            totalCreditos = items.Sum(i => i.MovCredito + i.SaldoAnteriorCredito),
            items
        });
    }
}

// ── BALANÇO PATRIMONIAL ───────────────────────────────────────────────────────

[ApiController]
[Route("api/balanco")]
[Authorize]
public class BalancoController : ControllerBase
{
    private readonly AppDbContext _db;
    public BalancoController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Obter(
        [FromQuery] int? ano = null,
        [FromQuery] int? mes = null)
    {
        var hoje   = DateTime.Today;
        var anoRef = ano ?? hoje.Year;
        var mesRef = mes ?? hoje.Month;
        var ate    = new DateTime(anoRef, mesRef, DateTime.DaysInMonth(anoRef, mesRef));

        decimal SaldoConta(string codigoPrefix, bool devedora)
        {
            var saldo = _db.LancamentoDetalhes
                .Include(d => d.Lancamento)
                .Include(d => d.ContaContabil)
                .Where(d => d.ContaContabil!.Codigo.StartsWith(codigoPrefix) && d.Lancamento!.Data <= ate)
                .Sum(d => devedora ? d.Debito - d.Credito : d.Credito - d.Debito);
            return Math.Max(0, saldo);
        }

        var ativoCorrentes = new List<BalancoLinhaResponse>
        {
            new("Caixa e Equivalentes de Caixa",      SaldoConta("11", true)),
            new("Clientes e Outras Contas a Receber",  SaldoConta("12", true)),
            new("Inventários e Activos Biológicos",    SaldoConta("13", true)),
            new("Outros Activos Correntes",            SaldoConta("15", true)),
        };

        var deprAcum = SaldoConta("64", true);
        var ativoNaoCorrentes = new List<BalancoLinhaResponse>
        {
            new("Activos Fixos Tangíveis (bruto)", SaldoConta("14", true)),
            new("(–) Depreciações Acumuladas",     -deprAcum, true),
            new("Outros Activos Não Correntes",    0),
        };

        var passivoCorrentes = new List<BalancoLinhaResponse>
        {
            new("Fornecedores e Contas a Pagar",    SaldoConta("21", false)),
            new("Empréstimos Bancários CP",         SaldoConta("22", false)),
            new("Imposto sobre o Rendimento (IRT)", SaldoConta("24", false)),
        };

        var passivoNaoCorrentes = new List<BalancoLinhaResponse>
        {
            new("Empréstimos Bancários LP",         0),
            new("Provisões para Riscos e Encargos", 0),
        };

        var capitalProprio = new List<BalancoLinhaResponse>
        {
            new("Capital Social",         SaldoConta("31", false)),
            new("Reservas Legais",        SaldoConta("32", false)),
            new("Resultados Transitados", SaldoConta("33", false)),
            new("Resultado do Exercício",
                SaldoConta("71", false) + SaldoConta("72", false) + SaldoConta("73", false)
                - SaldoConta("61", true) - SaldoConta("62", true) - SaldoConta("63", true)),
        };

        var totalAtivo              = ativoCorrentes.Sum(x => x.Valor) + ativoNaoCorrentes.Sum(x => x.Valor);
        var totalPassivo            = passivoCorrentes.Sum(x => x.Valor) + passivoNaoCorrentes.Sum(x => x.Valor);
        var totalCapital            = capitalProprio.Sum(x => x.Valor);
        var totalPassivoMaisCapital = totalPassivo + totalCapital;

        return Ok(new BalancoResponse(
            ativoCorrentes, ativoNaoCorrentes,
            passivoCorrentes, passivoNaoCorrentes, capitalProprio,
            totalAtivo, totalPassivo, totalCapital, totalPassivoMaisCapital));
    }
}

// ── DRE ───────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/dre")]
[Authorize]
public class DreController : ControllerBase
{
    private readonly AppDbContext _db;
    public DreController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Obter(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim    = null)
    {
        var inicio = dataInicio ?? new DateTime(DateTime.Today.Year, 1, 1);
        var fim    = dataFim    ?? DateTime.Today;

        var detalhes = await _db.LancamentoDetalhes
            .Include(d => d.Lancamento)
            .Include(d => d.ContaContabil)
            .Where(d => d.Lancamento!.Data >= inicio && d.Lancamento.Data <= fim &&
                        (d.ContaContabil!.Grupo == "Receita" || d.ContaContabil.Grupo == "Despesa"))
            .GroupBy(d => new { d.ContaContabil!.Codigo, d.ContaContabil.Nome, d.ContaContabil.Grupo })
            .Select(g => new
            {
                g.Key.Codigo, g.Key.Nome, g.Key.Grupo,
                Realizado = g.Key.Grupo == "Receita"
                    ? g.Sum(d => d.Credito - d.Debito)
                    : g.Sum(d => d.Debito  - d.Credito)
            })
            .ToListAsync();

        var linhas = detalhes.Select(d => new DreItemResponse(
            d.Codigo, d.Nome,
            d.Grupo == "Receita" ? "Receitas Operacionais" : "Custos Operacionais",
            d.Realizado * 0.9m,
            d.Realizado,
            d.Grupo == "Receita",
            fim)).ToList();

        var totalRec = linhas.Where(l => l.IsReceita).Sum(l => l.ValorRealizado);
        var totalCus = linhas.Where(l => !l.IsReceita).Sum(l => l.ValorRealizado);

        return Ok(new DreSumarioResponse(totalRec, totalCus, totalRec - totalCus, linhas));
    }
}
