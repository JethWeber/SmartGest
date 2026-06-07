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

[ApiController]
[Route("api/lancamentos")]
[Authorize]
public class LancamentosController : ControllerBase
{
    private readonly AppDbContext         _db;
    private readonly ContabilidadeService _contabilidade;

    public LancamentosController(AppDbContext db, ContabilidadeService contabilidade)
    {
        _db            = db;
        _contabilidade = contabilidade;
    }

    // ── GET /api/lancamentos ──────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string?   tipo       = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim    = null,
        [FromQuery] string?   texto      = null,
        [FromQuery] int?      contaId    = null,
        [FromQuery] bool      incluirAnulados = false,
        [FromQuery] int       pagina     = 1,
        [FromQuery] int       tamPagina  = 50)
    {
        var query = _db.Lancamentos.Include(l => l.ContaBancaria).AsQueryable();

        // Por defeito, oculta os lançamentos anulados (Soft Delete)
        if (!incluirAnulados)
            query = query.Where(l => !l.Anulado);

        if (!string.IsNullOrWhiteSpace(tipo))  query = query.Where(l => l.Tipo == tipo);
        if (dataInicio.HasValue)               query = query.Where(l => l.Data >= dataInicio.Value);
        if (dataFim.HasValue)                  query = query.Where(l => l.Data <= dataFim.Value);
        if (!string.IsNullOrWhiteSpace(texto)) query = query.Where(l =>
            l.Descricao.Contains(texto) || l.Categoria.Contains(texto) || l.Beneficiario.Contains(texto));
        if (contaId.HasValue)                  query = query.Where(l => l.ContaBancariaId == contaId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.Data)
            .Skip((pagina - 1) * tamPagina)
            .Take(tamPagina)
            .Select(l => Map(l))
            .ToListAsync();

        return Ok(new { total, pagina, tamPagina, items });
    }

    // ── GET /api/lancamentos/{id} ─────────────────────────────────────────────

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var l = await _db.Lancamentos
            .Include(x => x.ContaBancaria)
            .FirstOrDefaultAsync(x => x.Id == id);
        return l is null ? NotFound() : Ok(Map(l));
    }

    // ── POST /api/lancamentos ─────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] LancamentoRequest req)
    {
        if (req.Valor <= 0)
            return BadRequest(new { message = "O valor do lançamento deve ser maior que zero." });

        if (string.IsNullOrWhiteSpace(req.Tipo) || (req.Tipo != "Entrada" && req.Tipo != "Saída"))
            return BadRequest(new { message = "Tipo inválido. Use 'Entrada' ou 'Saída'." });

        // Verificação antecipada das contas contabilísticas
        try
        {
            var par    = MapeamentoContabil.Resolver(req.Categoria, req.Tipo);
            var contaD = await _db.ContasContabeis.AnyAsync(c => c.Codigo == par.CodigoDebito  && c.Activa);
            var contaC = await _db.ContasContabeis.AnyAsync(c => c.Codigo == par.CodigoCredito && c.Activa);
            if (!contaD || !contaC)
                return UnprocessableEntity(new
                {
                    message = $"Contas contabilísticas para a categoria '{req.Categoria}' ({req.Tipo}) " +
                              $"não encontradas (débito: {par.CodigoDebito}, crédito: {par.CodigoCredito}). " +
                              "Verifique o Plano de Contas PGC Angola."
                });
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }

        var lanc = new Lancamento
        {
            Data              = req.Data,
            Descricao         = req.Descricao,
            Categoria         = req.Categoria,
            Tipo              = req.Tipo,
            Valor             = req.Valor,
            Beneficiario      = req.Beneficiario      ?? string.Empty,
            MetodoPagamento   = req.MetodoPagamento   ?? string.Empty,
            CaminhoDocumento  = req.CaminhoDocumento  ?? string.Empty,
            Observacoes       = req.Observacoes       ?? string.Empty,
            CentroCusto       = req.CentroCusto       ?? string.Empty,
            ReferenciaInterna = req.ReferenciaInterna ?? string.Empty,
            ContaBancariaId   = req.ContaBancariaId,
            CriadoEm          = DateTime.UtcNow,
        };

        _db.Lancamentos.Add(lanc);
        await _db.SaveChangesAsync();

        // Actualizar saldo bancário
        if (req.ContaBancariaId.HasValue)
        {
            var conta = await _db.ContasBancarias.FindAsync(req.ContaBancariaId.Value);
            if (conta is not null)
            {
                conta.SaldoOntem  = conta.SaldoAtual;
                conta.SaldoAtual += req.Tipo == "Entrada" ? req.Valor : -req.Valor;
                await _db.SaveChangesAsync();
            }
        }

        // Gerar detalhes contabilísticos (partidas dobradas)
        try
        {
            await _contabilidade.GerarDetalhesAsync(lanc);
        }
        catch (Exception ex)
        {
            return CreatedAtAction(nameof(Obter), new { id = lanc.Id }, new
            {
                lancamento    = Map(lanc),
                avisoContabil = ex.Message
            });
        }

        return CreatedAtAction(nameof(Obter), new { id = lanc.Id }, Map(lanc));
    }

    // ── PUT /api/lancamentos/{id} ─────────────────────────────────────────────

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] LancamentoRequest req)
    {
        var lanc = await _db.Lancamentos.FindAsync(id);
        if (lanc is null) return NotFound();

        if (lanc.Anulado)
            return UnprocessableEntity(new
            {
                message = "Não é possível editar um lançamento anulado. " +
                          "Crie um novo lançamento de estorno se necessário."
            });

        var valorAntigo = lanc.Valor;
        var tipoAntigo  = lanc.Tipo;
        var contaAntiga = lanc.ContaBancariaId;

        lanc.Data              = req.Data;
        lanc.Descricao         = req.Descricao;
        lanc.Categoria         = req.Categoria;
        lanc.Tipo              = req.Tipo;
        lanc.Valor             = req.Valor;
        lanc.Beneficiario      = req.Beneficiario      ?? string.Empty;
        lanc.MetodoPagamento   = req.MetodoPagamento   ?? string.Empty;
        lanc.CaminhoDocumento  = req.CaminhoDocumento  ?? string.Empty;
        lanc.Observacoes       = req.Observacoes       ?? string.Empty;
        lanc.CentroCusto       = req.CentroCusto       ?? string.Empty;
        lanc.ReferenciaInterna = req.ReferenciaInterna ?? string.Empty;
        lanc.ContaBancariaId   = req.ContaBancariaId;

        await _db.SaveChangesAsync();

        // Reverter saldo antigo e aplicar novo
        if (contaAntiga.HasValue)
        {
            var contaOld = await _db.ContasBancarias.FindAsync(contaAntiga.Value);
            if (contaOld is not null)
                contaOld.SaldoAtual -= tipoAntigo == "Entrada" ? valorAntigo : -valorAntigo;
        }
        if (req.ContaBancariaId.HasValue)
        {
            var contaNova = await _db.ContasBancarias.FindAsync(req.ContaBancariaId.Value);
            if (contaNova is not null)
                contaNova.SaldoAtual += req.Tipo == "Entrada" ? req.Valor : -req.Valor;
        }
        await _db.SaveChangesAsync();

        try { await _contabilidade.GerarDetalhesAsync(lanc); }
        catch { /* não bloquear a actualização */ }

        return Ok(Map(lanc));
    }

    // ── DELETE /api/lancamentos/{id} — SOFT DELETE ────────────────────────────
    //
    // CONFORMIDADE FISCAL ANGOLANA — Decreto n.º 82/01
    // A eliminação física de lançamentos contabilísticos é proibida por lei,
    // pois destrói a trilha de auditoria exigida pelas autoridades fiscais.
    //
    // Este endpoint aplica Soft Delete: o lançamento permanece na base de dados
    // com o flag Anulado=true e os campos de auditoria preenchidos.
    // Os relatórios (Balancete, DRE, Balanço) ignoram registos com Anulado=true.
    //
    // Para reverter um lançamento errado, o procedimento correto em contabilidade
    // de partidas dobradas é criar um lançamento de ESTORNO (inverso).
    // ─────────────────────────────────────────────────────────────────────────

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Anular(int id, [FromQuery] string? motivo = null)
    {
        var lanc = await _db.Lancamentos.FindAsync(id);
        if (lanc is null) return NotFound();

        if (lanc.Anulado)
            return Conflict(new { message = "Este lançamento já se encontra anulado." });

        // Identificar o utilizador que está a anular (extraído do JWT)
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name) ?? "Sistema";

        // Marcar como anulado — SEM remoção física da base de dados
        lanc.Anulado        = true;
        lanc.AanuladoEm     = DateTime.UtcNow;
        lanc.AanuladoPor    = nomeUtilizador;
        lanc.MotivoAnulacao = motivo ?? "Anulado pelo utilizador.";

        // Reverter o impacto no saldo bancário, se aplicável
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
            message     = $"Lançamento #{id} anulado com sucesso. O registo contabilístico foi preservado para auditoria.",
            id          = lanc.Id,
            anuladoEm   = lanc.AanuladoEm,
            anuladoPor  = lanc.AanuladoPor,
            motivo      = lanc.MotivoAnulacao
        });
    }

    // ── POST /api/lancamentos/recalcular ──────────────────────────────────────

    [HttpPost("recalcular")]
    [Authorize(Roles = "Administrador,Contabilista")]
    public async Task<IActionResult> Recalcular()
    {
        var total = await _contabilidade.RecalcularLancamentosSemDetalhesAsync();
        return Ok(new { message = $"{total} lançamento(s) recalculado(s) com sucesso." });
    }

   // ── GET /api/lancamentos/categorias ──────────────────────────────────────

    [HttpGet("categorias")]
    [AllowAnonymous]
    public IActionResult ListarCategorias([FromQuery] string? tipo = null)
    {
        var lista = MapeamentoContabil.ListarCategorias(tipo);
        return Ok(lista);
    }

    // ── HELPER ────────────────────────────────────────────────────────────────

    private static LancamentoResponse Map(Lancamento l) => new(
        l.Id, l.Data, l.Descricao, l.Categoria, l.Tipo, l.Valor,
        l.Beneficiario, l.MetodoPagamento, l.CaminhoDocumento,
        l.Observacoes, l.CentroCusto, l.ReferenciaInterna, l.CriadoEm,
        l.ContaBancariaId, l.ContaBancaria?.Banco);
    
}
