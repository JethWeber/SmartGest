using System.ComponentModel.DataAnnotations;

namespace SmartGest.API.Models;

// ── UTILIZADOR / AUTH ─────────────────────────────────────────────────────────
public class Utilizador
{
    public int      Id           { get; set; }
    [Required, MaxLength(150)]
    public string   Nome         { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string   Email        { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string   Telefone     { get; set; } = string.Empty;
    [Required]
    public string   PasswordHash { get; set; } = string.Empty;
    [MaxLength(30)]
    public string   Perfil       { get; set; } = "Operador"; // Administrador | Contabilista | Operador | Visualizador
    public bool     Activo       { get; set; } = true;
    public string   Iniciais     { get; set; } = string.Empty;
    public string   CorAvatar    { get; set; } = "#1A2E5A";
    public DateTime CriadoEm    { get; set; }

    public ICollection<SessaoActiva> Sessoes { get; set; } = new List<SessaoActiva>();
}

public class SessaoActiva
{
    public int      Id               { get; set; }
    public int      UtilizadorId     { get; set; }
    public string   Dispositivo      { get; set; } = string.Empty;
    public string   Localizacao      { get; set; } = string.Empty;
    public DateTime UltimaActividade { get; set; }
    public bool     IsAtual          { get; set; } = false;

    public Utilizador? Utilizador { get; set; }
}

// ── EMPRESA ───────────────────────────────────────────────────────────────────
public class Empresa
{
    public int     Id       { get; set; }
    [Required, MaxLength(200)]
    public string  Nome     { get; set; } = string.Empty;
    [MaxLength(30)]
    public string  NIF      { get; set; } = string.Empty;
    [MaxLength(300)]
    public string  Morada   { get; set; } = string.Empty;
    [MaxLength(100)]
    public string  Cidade   { get; set; } = string.Empty;
    [MaxLength(100)]
    public string  Pais     { get; set; } = "Angola";
    [MaxLength(30)]
    public string  Telefone { get; set; } = string.Empty;
    [MaxLength(150)]
    public string  Email    { get; set; } = string.Empty;
    [MaxLength(200)]
    public string  Website  { get; set; } = string.Empty;
    public decimal Capital  { get; set; }
    public string? LogoPath { get; set; }
}

// ── CONFIGURAÇÕES ─────────────────────────────────────────────────────────────
public class Configuracao
{
    public int     Id                   { get; set; }
    // Aparência
    public int     TemaIndex            { get; set; } = 0;
    public int     IdiomaIndex          { get; set; } = 0;
    public int     MoedaIndex           { get; set; } = 0;
    public int     DataFormatoIndex     { get; set; } = 0;
    public bool    MostrarSparklines    { get; set; } = true;
    public bool    AnimacoesAtivadas    { get; set; } = true;
    public bool    MostrarSaldosOcultos { get; set; } = false;
    // Notificações
    public bool    NotifEmail           { get; set; } = true;
    public bool    NotifApp             { get; set; } = true;
    public bool    NotifSaldoBaixo      { get; set; } = true;
    public bool    NotifLancamentos     { get; set; } = true;
    public bool    NotifRelatorios      { get; set; } = false;
    public bool    NotifErrosSistema    { get; set; } = true;
    public bool    NotifBackup          { get; set; } = true;
    public string  EmailNotificacoes    { get; set; } = string.Empty;
    public decimal LimiarSaldoBaixo     { get; set; } = 500_000;
    // Segurança
    public bool    DoisFatoresAtivo     { get; set; } = false;
    public int     SessaoTimeoutMins    { get; set; } = 30;
    public bool    RegistarAuditoria    { get; set; } = true;
    // API
    public string  ApiBaseUrl           { get; set; } = string.Empty;
    public string  ApiKey               { get; set; } = string.Empty;
    public int     TimeoutIndex         { get; set; } = 1;
    public bool    TlsAtivado           { get; set; } = true;
    public bool    RetryAtivado         { get; set; } = true;
}

public class Webhook
{
    public int    Id     { get; set; }
    [MaxLength(100)]
    public string Evento { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Url    { get; set; } = string.Empty;
    public bool   Activo { get; set; } = false;
}

// ── PLANO DE CONTAS ───────────────────────────────────────────────────────────
public class ContaContabil
{
    public int    Id         { get; set; }
    [Required, MaxLength(20)]
    public string Codigo     { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Nome       { get; set; } = string.Empty;
    [Required, MaxLength(30)]
    public string Grupo      { get; set; } = string.Empty; // Ativo | Passivo | Capital | Receita | Despesa
    public bool   IsDevedora { get; set; } = true;
    public bool   Activa     { get; set; } = true;

    public ICollection<LancamentoDetalhe> Lancamentos { get; set; } = new List<LancamentoDetalhe>();
}

// ── LANÇAMENTOS ───────────────────────────────────────────────────────────────
public class Lancamento
{
    public int      Id                { get; set; }
    [Required]
    public DateTime Data              { get; set; }
    [Required, MaxLength(500)]
    public string   Descricao         { get; set; } = string.Empty;
    [MaxLength(50)]
    public string   Categoria         { get; set; } = string.Empty;
    public int?      CategoriaContabilId { get; set; }
    public CategoriaContabil? CategoriaContabil { get; set; }
    [MaxLength(30)]
    public string   Tipo              { get; set; } = string.Empty; // Entrada | Saída
    public decimal  Valor             { get; set; }
    [MaxLength(100)]
    public string   Beneficiario      { get; set; } = string.Empty;
    [MaxLength(50)]
    public string   MetodoPagamento   { get; set; } = string.Empty;
    [MaxLength(200)]
    public string   CaminhoDocumento  { get; set; } = string.Empty;
    [MaxLength(500)]
    public string   Observacoes       { get; set; } = string.Empty;
    [MaxLength(100)]
    public string   CentroCusto       { get; set; } = string.Empty;
    [MaxLength(100)]
    public string   ReferenciaInterna { get; set; } = string.Empty;
    public decimal  ImpostoSelo       { get; set; }
    public DateTime CriadoEm         { get; set; }

    // ── SOFT DELETE — Conformidade fiscal angolana ──────────────────────────
    // Lançamentos contabilísticos nunca são eliminados fisicamente do banco.
    // A anulação deve ser feita por estorno (lançamento inverso) ou marcação.
    // AanuladoEm e AanuladoPor garantem a trilha de auditoria exigida por lei.
    public bool      Anulado      { get; set; } = false;
    public DateTime? AanuladoEm   { get; set; }
    [MaxLength(150)]
    public string?   AanuladoPor  { get; set; }
    [MaxLength(500)]
    public string?   MotivoAnulacao { get; set; }

    public int?           ContaBancariaId { get; set; }
    public ContaBancaria? ContaBancaria   { get; set; }

    public ICollection<LancamentoDetalhe> Detalhes { get; set; } = new List<LancamentoDetalhe>();
}

public class LancamentoDetalhe
{
    public int     Id              { get; set; }
    public int     LancamentoId   { get; set; }
    public int     ContaContabilId { get; set; }
    public decimal Debito          { get; set; }
    public decimal Credito         { get; set; }

    public Lancamento?    Lancamento    { get; set; }
    public ContaContabil? ContaContabil { get; set; }
}

// ── CONTAS BANCÁRIAS ──────────────────────────────────────────────────────────
public class ContaBancaria
{
    public int     Id        { get; set; }
    [Required, MaxLength(100)]
    public string  Banco     { get; set; } = string.Empty;
    [MaxLength(50)]
    public string  NIB       { get; set; } = string.Empty;
    [MaxLength(50)]
    public string  Tipo      { get; set; } = "Conta à Ordem";
    [MaxLength(10)]
    public string  Moeda     { get; set; } = "AOA";
    public decimal SaldoAtual { get; set; }
    public decimal SaldoOntem { get; set; }
    [MaxLength(150)]
    public string  Agencia   { get; set; } = string.Empty;
    [MaxLength(200)]
    public string  Titular   { get; set; } = string.Empty;
    [MaxLength(20)]
    public string  CorAccent { get; set; } = "#1A2E5A";
    public bool    Activa    { get; set; } = true;
    public int?          ContaContabilId { get; set; }
    public ContaContabil? ContaContabil  { get; set; }

    public ICollection<MovimentoBancario> Movimentos  { get; set; } = new List<MovimentoBancario>();
    public ICollection<Lancamento>        Lancamentos { get; set; } = new List<Lancamento>();
}

public class MovimentoBancario
{
    public int      Id              { get; set; }
    public int      ContaBancariaId { get; set; }
    [Required]
    public DateTime Data            { get; set; }
    [Required, MaxLength(500)]
    public string   Descricao       { get; set; } = string.Empty;
    [MaxLength(50)]
    public string   Referencia      { get; set; } = string.Empty;
    [MaxLength(20)]
    public string   Tipo            { get; set; } = string.Empty; // Crédito | Débito
    public decimal  Valor           { get; set; }

    public ContaBancaria? ContaBancaria { get; set; }
}
