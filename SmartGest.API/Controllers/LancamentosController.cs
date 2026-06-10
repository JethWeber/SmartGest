using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.DTOs.Requests;
using SmartGest.API.DTOs.Responses;
using SmartGest.API.Models;
using SmartGest.API.Services;
using System.Security.Claims;

namespace SmartGest.API.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// CATEGORIAS CONTABILÍSTICAS
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriasController(AppDbContext db) => _db = db;

    /// <summary>Lista todas as categorias, opcionalmente filtradas por tipo.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Listar([FromQuery] string? tipo = null)
    {
        var query = _db.CategoriaContabeis.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(c => c.Tipo == tipo);

        var items = await query
            .Where(c => c.Ativo)
            .OrderBy(c => c.Tipo)
            .ThenBy(c => c.Nome)
            .Select(c => new CategoriaResponse(
                c.Id, c.Nome, c.Tipo,
                c.GrupoDre, c.GrupoBalanco, c.GrupoFluxoCaixa,
                c.AplicaImpostoSelo))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var c = await _db.CategoriaContabeis.FindAsync(id);
        if (c is null) return NotFound();
        return Ok(new CategoriaResponse(
            c.Id, c.Nome, c.Tipo,
            c.GrupoDre, c.GrupoBalanco, c.GrupoFluxoCaixa,
            c.AplicaImpostoSelo));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Criar([FromBody] CategoriaRequest req)
    {
        var cat = new CategoriaContabil
        {
            Nome              = req.Nome,
            Tipo              = req.Tipo,
            ContaDebito       = req.ContaDebito,
            ContaCredito      = req.ContaCredito,
            GrupoDre          = req.GrupoDre          ?? string.Empty,
            GrupoBalanco      = req.GrupoBalanco      ?? string.Empty,
            GrupoFluxoCaixa   = req.GrupoFluxoCaixa   ?? string.Empty,
            AplicaImpostoSelo = req.AplicaImpostoSelo,
        };
        _db.CategoriaContabeis.Add(cat);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obter), new { id = cat.Id },
            new CategoriaResponse(cat.Id, cat.Nome, cat.Tipo,
                cat.GrupoDre, cat.GrupoBalanco, cat.GrupoFluxoCaixa, cat.AplicaImpostoSelo));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] CategoriaRequest req)
    {
        var cat = await _db.CategoriaContabeis.FindAsync(id);
        if (cat is null) return NotFound();

        cat.Nome              = req.Nome;
        cat.Tipo              = req.Tipo;
        cat.ContaDebito       = req.ContaDebito;
        cat.ContaCredito      = req.ContaCredito;
        cat.GrupoDre          = req.GrupoDre          ?? string.Empty;
        cat.GrupoBalanco      = req.GrupoBalanco      ?? string.Empty;
        cat.GrupoFluxoCaixa   = req.GrupoFluxoCaixa   ?? string.Empty;
        cat.AplicaImpostoSelo = req.AplicaImpostoSelo;

        await _db.SaveChangesAsync();
        return Ok(new CategoriaResponse(cat.Id, cat.Nome, cat.Tipo,
            cat.GrupoDre, cat.GrupoBalanco, cat.GrupoFluxoCaixa, cat.AplicaImpostoSelo));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var cat = await _db.CategoriaContabeis.FindAsync(id);
        if (cat is null) return NotFound();
        cat.Ativo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// LANÇAMENTOS — V2 (motor contabilístico centralizado)
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/lancamentos")]
[Authorize]
public class LancamentosController : ControllerBase
{
    private readonly AppDbContext  _db;
    private readonly IMotorContabil _motor;

    public LancamentosController(AppDbContext db, IMotorContabil motor)
    {
        _db    = db;
        _motor = motor;
    }

    // ── GET /api/lancamentos ─────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string?   tipo            = null,
        [FromQuery] DateTime? dataInicio       = null,
        [FromQuery] DateTime? dataFim          = null,
        [FromQuery] string?   texto            = null,
        [FromQuery] int?      contaBancariaId  = null,
        [FromQuery] int?      categoriaId      = null,
        [FromQuery] bool      incluirAnulados  = false,
        [FromQuery] int       pagina           = 1,
        [FromQuery] int       tamPagina        = 50)
    {
        var query = _db.Lancamentos.Include(l => l.ContaBancaria).AsQueryable();

        if (!incluirAnulados)       query = query.Where(l => !l.Anulado);
        if (tipo is not null)       query = query.Where(l => l.Tipo == tipo);
        if (dataInicio.HasValue)    query = query.Where(l => l.Data >= dataInicio.Value);
        if (dataFim.HasValue)       query = query.Where(l => l.Data <= dataFim.Value);
        if (contaBancariaId.HasValue) query = query.Where(l => l.ContaBancariaId == contaBancariaId);
        if (categoriaId.HasValue)   query = query.Where(l => l.CategoriaContabilId == categoriaId);

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(l =>
                l.Descricao.Contains(texto) ||
                l.Beneficiario.Contains(texto) ||
                l.ReferenciaInterna.Contains(texto));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.Data)
            .Skip((pagina - 1) * tamPagina)
            .Take(tamPagina)
            .Select(l => Map(l))
            .ToListAsync();

        return Ok(new { total, pagina, tamPagina, items });
    }

    // ── GET /api/lancamentos/{id} ────────────────────────────────────────────

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var l = await _db.Lancamentos
            .Include(x => x.ContaBancaria)
            .FirstOrDefaultAsync(x => x.Id == id);
        return l is null ? NotFound() : Ok(Map(l));
    }

    // ── POST /api/lancamentos ────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] LancamentoRequest req)
    {
        // ── Validações de entrada ────────────────────────────────────────────
        if (req.Valor <= 0)
            return BadRequest(new { message = "O valor deve ser maior que zero." });

        if (req.Tipo is not ("Entrada" or "Saída"))
            return BadRequest(new { message = "Tipo inválido. Use 'Entrada' ou 'Saída'." });

        // ── CategoriaId é OBRIGATÓRIO no V2 ─────────────────────────────────
            var categoria = await _db.CategoriaContabeis.FindAsync(req.CategoriaId);
        if (categoria is null || !categoria.Ativo)
            return UnprocessableEntity(new { message = "Categoria inválida ou inactiva." });

        if (categoria.Tipo != req.Tipo)
            return UnprocessableEntity(new
            {
                message = $"A categoria '{categoria.Nome}' é do tipo '{categoria.Tipo}' " +
                          $"mas o lançamento foi enviado como '{req.Tipo}'. Corrija o tipo."
            });

        // ── Verificar contas PGC existem antes de gravar ─────────────────────
        var contaD = await _db.ContasContabeis.AnyAsync(c => c.Codigo == categoria.ContaDebito  && c.Activa);
        var contaC = await _db.ContasContabeis.AnyAsync(c => c.Codigo == categoria.ContaCredito && c.Activa);
        if (!contaD || !contaC)
            return UnprocessableEntity(new
            {
                message = $"Contas PGC não encontradas (débito: {categoria.ContaDebito}, " +
                          $"crédito: {categoria.ContaCredito}). Verifique o Plano de Contas."
            });

        // ── Persistir lançamento ─────────────────────────────────────────────
        var lanc = new Lancamento
        {
            Data                = req.Data,
            Descricao           = req.Descricao,
            Categoria           = categoria.Nome,        // nome legível (preservado para auditoria)
            CategoriaContabilId = req.CategoriaId,
            Tipo                = req.Tipo,
            Valor               = req.Valor,
            Beneficiario        = req.Beneficiario      ?? string.Empty,
            MetodoPagamento     = req.MetodoPagamento   ?? string.Empty,
            CaminhoDocumento    = req.CaminhoDocumento  ?? string.Empty,
            Observacoes         = req.Observacoes       ?? string.Empty,
            CentroCusto         = req.CentroCusto       ?? string.Empty,
            ReferenciaInterna   = req.ReferenciaInterna ?? string.Empty,
            ContaBancariaId     = req.ContaBancariaId,
            CriadoEm            = DateTime.UtcNow,
        };

        _db.Lancamentos.Add(lanc);
        await _db.SaveChangesAsync();

        // ── Actualizar saldo bancário ─────────────────────────────────────────
        await AtualizarSaldoBancarioAsync(null, req.ContaBancariaId, null, req.Tipo, req.Valor);

        // ── Motor contabilístico: gerar partidas dobradas ─────────────────────
        try
        {
            await _motor.ProcessarAsync(lanc);
        }
        catch (Exception ex)
        {
            // O lançamento está gravado mas sem detalhes — retornamos 201 com aviso.
            return CreatedAtAction(nameof(Obter), new { id = lanc.Id }, new
            {
                lancamento    = Map(lanc),
                avisoContabil = ex.Message
            });
        }

        return CreatedAtAction(nameof(Obter), new { id = lanc.Id }, Map(lanc));
    }

    // ── PUT /api/lancamentos/{id} ────────────────────────────────────────────

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] LancamentoRequest req)
    {
        var lanc = await _db.Lancamentos.FindAsync(id);
        if (lanc is null) return NotFound();

        if (lanc.Anulado)
            return UnprocessableEntity(new
            {
                message = "Não é possível editar um lançamento anulado. " +
                          "Crie um lançamento de estorno se necessário."
            });

        var categoria = await _db.CategoriaContabeis.FindAsync(req.CategoriaId);
        if (categoria is null || !categoria.Ativo)
            return UnprocessableEntity(new { message = "Categoria inválida ou inactiva." });

        // Reverter saldo antigo e aplicar novo
        await AtualizarSaldoBancarioAsync(
            lanc.ContaBancariaId, req.ContaBancariaId,
            (lanc.Tipo, lanc.Valor),
            req.Tipo, req.Valor);

        lanc.Data               = req.Data;
        lanc.Descricao          = req.Descricao;
        lanc.Categoria          = categoria.Nome;
        lanc.CategoriaContabilId = req.CategoriaId;
        lanc.Tipo               = req.Tipo;
        lanc.Valor              = req.Valor;
        lanc.Beneficiario       = req.Beneficiario      ?? string.Empty;
        lanc.MetodoPagamento    = req.MetodoPagamento   ?? string.Empty;
        lanc.CaminhoDocumento   = req.CaminhoDocumento  ?? string.Empty;
        lanc.Observacoes        = req.Observacoes       ?? string.Empty;
        lanc.CentroCusto        = req.CentroCusto       ?? string.Empty;
        lanc.ReferenciaInterna  = req.ReferenciaInterna ?? string.Empty;
        lanc.ContaBancariaId    = req.ContaBancariaId;

        await _db.SaveChangesAsync();

        try { await _motor.ProcessarAsync(lanc); }
        catch { /* não bloquear actualização */ }

        return Ok(Map(lanc));
    }

    // ── DELETE /api/lancamentos/{id} — SOFT DELETE ───────────────────────────
    //
    // Conformidade fiscal angolana — Decreto n.º 82/01.
    // Lançamentos contabilísticos nunca são eliminados fisicamente.
    // ────────────────────────────────────────────────────────────────────────

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Anular(int id, [FromQuery] string? motivo = null)
    {
        var lanc = await _db.Lancamentos.FindAsync(id);
        if (lanc is null) return NotFound();

        if (lanc.Anulado)
            return Conflict(new { message = "Este lançamento já se encontra anulado." });

        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name) ?? "Sistema";

        lanc.Anulado        = true;
        lanc.AanuladoEm     = DateTime.UtcNow;
        lanc.AanuladoPor    = nomeUtilizador;
        lanc.MotivoAnulacao = motivo ?? "Anulado pelo utilizador.";

        // Reverter saldo bancário
        if (lanc.ContaBancariaId.HasValue)
        {
            var conta = await _db.ContasBancarias.FindAsync(lanc.ContaBancariaId.Value);
            if (conta is not null)
            {
                conta.SaldoOntem  = conta.SaldoAtual;
                conta.SaldoAtual -= lanc.Tipo == "Entrada" ? lanc.Valor : -lanc.Valor;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message    = $"Lançamento #{id} anulado. Registo preservado para auditoria.",
            id         = lanc.Id,
            anuladoEm  = lanc.AanuladoEm,
            anuladoPor = lanc.AanuladoPor,
            motivo     = lanc.MotivoAnulacao
        });
    }

    // ── POST /api/lancamentos/recalcular ─────────────────────────────────────

    [HttpPost("recalcular")]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Recalcular()
    {
        var total = await _motor.RecalcularSemDetalhesAsync();
        return Ok(new { message = $"{total} lançamento(s) recalculado(s) com sucesso." });
    }

    // ── GET /api/lancamentos/categorias (retrocompatibilidade) ───────────────

    [HttpGet("categorias")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarCategorias([FromQuery] string? tipo = null)
    {
        var query = _db.CategoriaContabeis.Where(c => c.Ativo);
        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(c => c.Tipo == tipo);

        var lista = await query
            .OrderBy(c => c.Tipo).ThenBy(c => c.Nome)
            .Select(c => new { c.Id, c.Nome, c.Tipo })
            .ToListAsync();

        return Ok(lista);
    }

    // ── HELPERS ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Trata actualizações de saldo bancário para criar e editar lançamentos.
    /// Se anterior != null, reverte o impacto anterior antes de aplicar o novo.
    /// </summary>
    private async Task AtualizarSaldoBancarioAsync(
        int?    contaAntigaId,
        int?    contaNovaId,
        (string Tipo, decimal Valor)? anterior,
        string  novoTipo,
        decimal novoValor)
    {
        // Reverter impacto anterior
        if (contaAntigaId.HasValue && anterior.HasValue)
        {
            var contaAnt = await _db.ContasBancarias.FindAsync(contaAntigaId.Value);
            if (contaAnt is not null)
                contaAnt.SaldoAtual -= anterior.Value.Tipo == "Entrada"
                    ? anterior.Value.Valor
                    : -anterior.Value.Valor;
        }

        // Aplicar novo impacto
        if (contaNovaId.HasValue)
        {
            var contaNova = await _db.ContasBancarias.FindAsync(contaNovaId.Value);
            if (contaNova is not null)
            {
                contaNova.SaldoOntem  = contaNova.SaldoAtual;
                contaNova.SaldoAtual += novoTipo == "Entrada" ? novoValor : -novoValor;
            }
        }

        await _db.SaveChangesAsync();
    }

    private static LancamentoResponse Map(Lancamento l) => new(
        l.Id, l.Data, l.Descricao, l.Categoria, l.Tipo, l.Valor,
        l.Beneficiario, l.MetodoPagamento, l.CaminhoDocumento,
        l.Observacoes, l.CentroCusto, l.ReferenciaInterna, l.CriadoEm,
        l.ContaBancariaId, l.ContaBancaria?.Banco);
}
