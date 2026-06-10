using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.DTOs.Requests;
using SmartGest.API.DTOs.Responses;
using SmartGest.API.Models;

namespace SmartGest.API.Controllers;

// ── UTILIZADORES ──────────────────────────────────────────────────────────────

[ApiController]
[Route("api/utilizadores")]
[Authorize]
public class UtilizadoresController : ControllerBase
{
    private readonly AppDbContext _db;
    public UtilizadoresController(AppDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Listar()
    {
        var items = await _db.Utilizadores
            .OrderBy(u => u.Nome)
            .Select(u => Map(u))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var u = await _db.Utilizadores.FindAsync(id);
        return u is null ? NotFound() : Ok(Map(u));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Criar([FromBody] CriarUtilizadorRequest req)
    {
        if (await _db.Utilizadores.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "Email já registado." });

        var partes   = req.Nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var iniciais = partes.Length >= 2
            ? $"{partes[0][0]}{partes[^1][0]}"
            : req.Nome.Length >= 2 ? req.Nome[..2].ToUpper() : req.Nome.ToUpper();

        var user = new Utilizador
        {
            Nome         = req.Nome,
            Email        = req.Email,
            Telefone     = req.Telefone,
            Perfil       = req.Perfil,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Iniciais     = iniciais,
        };

        _db.Utilizadores.Add(user);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obter), new { id = user.Id }, Map(user));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUtilizadorRequest req)
    {
        var u = await _db.Utilizadores.FindAsync(id);
        if (u is null) return NotFound();

        u.Nome   = req.Nome;
        u.Email  = req.Email;
        u.Perfil = req.Perfil;
        u.Activo = req.Activo;
        await _db.SaveChangesAsync();
        return Ok(Map(u));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var u = await _db.Utilizadores.FindAsync(id);
        if (u is null) return NotFound();
        u.Activo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static UtilizadorResponse Map(Utilizador u) => new(
        u.Id, u.Nome, u.Email, u.Telefone, u.Perfil, u.Activo, u.Iniciais, u.CorAvatar, u.CriadoEm);
}

// ── DASHBOARD ─────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var hoje      = DateTime.Today;
        var inicioAno = new DateTime(hoje.Year, 1, 1);
        var anoPass   = hoje.Year - 1;

        // FIX #2: filtro por intervalo de datas — aproveita o índice IX_Lancamento_Data
        // e é imune ao bug de UTC (sem .Year == anoAtual que força EXTRACT no PostgreSQL)
        var lancamentos = await _db.Lancamentos
            .Where(l => !l.Anulado && l.Data >= inicioAno && l.Data <= hoje)
            .ToListAsync();

        // FIX #2.3: ano anterior com intervalo equivalente — sem lógica frágil de dias
        var inicioAnoPass = new DateTime(anoPass, 1, 1);
        var fimAnoPass    = new DateTime(
            anoPass, hoje.Month,
            Math.Min(hoje.Day, DateTime.DaysInMonth(anoPass, hoje.Month)));

        var lancamentosAnoPassado = await _db.Lancamentos
            .Where(l => !l.Anulado && l.Data >= inicioAnoPass && l.Data <= fimAnoPass)
            .ToListAsync();

        var meses = Enumerable.Range(1, 12).Select(m =>
        {
            var nomeMes = new[] { "Jan","Fev","Mar","Abr","Mai","Jun",
                                  "Jul","Ago","Set","Out","Nov","Dez" }[m - 1];
            var rec = lancamentos.Where(l => l.Data.Month == m && l.Tipo == "Entrada").Sum(l => l.Valor);
            var des = lancamentos.Where(l => l.Data.Month == m && l.Tipo == "Saída").Sum(l => l.Valor);
            return new FluxoMensalItem(nomeMes, rec, des, rec - des);
        }).ToList();

        var totalRec  = lancamentos.Where(l => l.Tipo == "Entrada").Sum(l => l.Valor);
        var totalDesp = lancamentos.Where(l => l.Tipo == "Saída").Sum(l => l.Valor);

        var recAnoPass  = lancamentosAnoPassado.Where(l => l.Tipo == "Entrada").Sum(l => l.Valor);
        var despAnoPass = lancamentosAnoPassado.Where(l => l.Tipo == "Saída").Sum(l => l.Valor);

        var ultimas = await _db.Lancamentos
            .Include(l => l.ContaBancaria)
            .Where(l => !l.Anulado)
            .OrderByDescending(l => l.Data)
            .Take(10)
            .Select(l => new LancamentoResponse(
                l.Id, l.Data, l.Descricao, l.Categoria, l.Tipo, l.Valor,
                l.Beneficiario, l.MetodoPagamento, l.CaminhoDocumento,
                l.Observacoes, l.CentroCusto, l.ReferenciaInterna, l.CriadoEm,
                l.ContaBancariaId, l.ContaBancaria != null ? l.ContaBancaria.Banco : null))
            .ToListAsync();

        return Ok(new DashboardResponse(
            totalRec, totalDesp, totalRec - totalDesp,
            recAnoPass, despAnoPass, recAnoPass - despAnoPass,
            meses, ultimas));
    }
}

// ── EMPRESA ───────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/empresa")]
[Authorize]
public class EmpresaController : ControllerBase
{
    private readonly AppDbContext _db;
    public EmpresaController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var e = await _db.Empresas.FirstOrDefaultAsync();
        if (e is null) return NotFound();
        return Ok(new EmpresaResponse(e.Id, e.Nome, e.NIF, e.Morada, e.Cidade,
            e.Pais, e.Telefone, e.Email, e.Website, e.Capital, e.LogoPath));
    }

    [HttpPut]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Atualizar([FromBody] EmpresaRequest req)
    {
        var e = await _db.Empresas.FirstOrDefaultAsync() ?? new Empresa();
        e.Nome = req.Nome; e.NIF = req.NIF; e.Morada = req.Morada;
        e.Cidade = req.Cidade; e.Pais = req.Pais; e.Telefone = req.Telefone;
        e.Email = req.Email; e.Website = req.Website; e.Capital = req.Capital;

        if (e.Id == 0) _db.Empresas.Add(e);
        await _db.SaveChangesAsync();
        return Ok(new EmpresaResponse(e.Id, e.Nome, e.NIF, e.Morada, e.Cidade,
            e.Pais, e.Telefone, e.Email, e.Website, e.Capital, e.LogoPath));
    }
}

// ── CONFIGURAÇÕES ─────────────────────────────────────────────────────────────

[ApiController]
[Route("api/configuracoes")]
[Authorize]
public class ConfiguracoesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ConfiguracoesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var c = await _db.Configuracoes.FirstOrDefaultAsync() ?? new Configuracao();
        return Ok(MapCfg(c));
    }

    [HttpPut("aparencia")]
    public async Task<IActionResult> SalvarAparencia([FromBody] AparenciaRequest req)
    {
        var c = await _db.Configuracoes.FirstOrDefaultAsync() ?? new Configuracao();
        c.TemaIndex = req.TemaIndex; c.IdiomaIndex = req.IdiomaIndex;
        c.MoedaIndex = req.MoedaIndex; c.DataFormatoIndex = req.DataFormatoIndex;
        c.MostrarSparklines = req.MostrarSparklines;
        c.AnimacoesAtivadas = req.AnimacoesAtivadas;
        c.MostrarSaldosOcultos = req.MostrarSaldosOcultos;
        if (c.Id == 0) _db.Configuracoes.Add(c);
        await _db.SaveChangesAsync();
        return Ok(MapCfg(c));
    }

    [HttpPut("notificacoes")]
    public async Task<IActionResult> SalvarNotificacoes([FromBody] NotificacoesRequest req)
    {
        var c = await _db.Configuracoes.FirstOrDefaultAsync() ?? new Configuracao();
        c.NotifEmail = req.NotifEmail; c.NotifApp = req.NotifApp;
        c.NotifSaldoBaixo = req.NotifSaldoBaixo; c.NotifLancamentos = req.NotifLancamentos;
        c.NotifRelatorios = req.NotifRelatorios; c.NotifErrosSistema = req.NotifErrosSistema;
        c.NotifBackup = req.NotifBackup;
        c.EmailNotificacoes = req.EmailNotificacoes;
        c.LimiarSaldoBaixo = req.LimiarSaldoBaixo;
        if (c.Id == 0) _db.Configuracoes.Add(c);
        await _db.SaveChangesAsync();
        return Ok(MapCfg(c));
    }

    [HttpPut("seguranca")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> SalvarSeguranca([FromBody] SegurancaRequest req)
    {
        var c = await _db.Configuracoes.FirstOrDefaultAsync() ?? new Configuracao();
        c.DoisFatoresAtivo = req.DoisFatoresAtivo;
        c.SessaoTimeoutMins = req.SessaoTimeoutMins;
        c.RegistarAuditoria = req.RegistarAuditoria;
        if (c.Id == 0) _db.Configuracoes.Add(c);
        await _db.SaveChangesAsync();
        return Ok(MapCfg(c));
    }

    [HttpPut("api")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> SalvarApi([FromBody] ApiIntegracaoRequest req)
    {
        var c = await _db.Configuracoes.FirstOrDefaultAsync() ?? new Configuracao();
        c.ApiBaseUrl = req.ApiBaseUrl; c.TimeoutIndex = req.TimeoutIndex;
        c.TlsAtivado = req.TlsAtivado; c.RetryAtivado = req.RetryAtivado;
        if (c.Id == 0) _db.Configuracoes.Add(c);
        await _db.SaveChangesAsync();
        return Ok(MapCfg(c));
    }

    [HttpGet("webhooks")]
    public async Task<IActionResult> ListarWebhooks()
    {
        var items = await _db.Webhooks
            .Select(w => new WebhookResponse(w.Id, w.Evento, w.Url, w.Activo))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("webhooks")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CriarWebhook([FromBody] WebhookRequest req)
    {
        var w = new Webhook { Evento = req.Evento, Url = req.Url, Activo = req.Activo };
        _db.Webhooks.Add(w);
        await _db.SaveChangesAsync();
        return Ok(new WebhookResponse(w.Id, w.Evento, w.Url, w.Activo));
    }

    [HttpPut("webhooks/{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> AtualizarWebhook(int id, [FromBody] WebhookRequest req)
    {
        var w = await _db.Webhooks.FindAsync(id);
        if (w is null) return NotFound();
        w.Evento = req.Evento; w.Url = req.Url; w.Activo = req.Activo;
        await _db.SaveChangesAsync();
        return Ok(new WebhookResponse(w.Id, w.Evento, w.Url, w.Activo));
    }

    [HttpDelete("webhooks/{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> EliminarWebhook(int id)
    {
        var w = await _db.Webhooks.FindAsync(id);
        if (w is null) return NotFound();
        _db.Webhooks.Remove(w);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ConfiguracaoResponse MapCfg(Configuracao c) => new(
        c.Id, c.TemaIndex, c.IdiomaIndex, c.MoedaIndex, c.DataFormatoIndex,
        c.MostrarSparklines, c.AnimacoesAtivadas, c.MostrarSaldosOcultos,
        c.NotifEmail, c.NotifApp, c.NotifSaldoBaixo, c.NotifLancamentos,
        c.NotifRelatorios, c.NotifErrosSistema, c.NotifBackup,
        c.EmailNotificacoes, c.LimiarSaldoBaixo,
        c.DoisFatoresAtivo, c.SessaoTimeoutMins, c.RegistarAuditoria,
        c.ApiBaseUrl, c.TimeoutIndex, c.TlsAtivado, c.RetryAtivado);
}
