using Microsoft.EntityFrameworkCore;
using SmartGest.Core.Domain;

namespace SmartGest.Infrastructure.Persistence;

public sealed class SmartGestDbContext : DbContext
{
    public SmartGestDbContext(DbContextOptions<SmartGestDbContext> options) : base(options) { }

    public DbSet<Utilizador> Utilizadores => Set<Utilizador>();
    public DbSet<SessaoActiva> Sessoes => Set<SessaoActiva>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Configuracao> Configuracoes => Set<Configuracao>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<ContaContabil> ContasContabeis => Set<ContaContabil>();
    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<LancamentoDetalhe> LancamentoDetalhes => Set<LancamentoDetalhe>();
    public DbSet<ContaBancaria> ContasBancarias => Set<ContaBancaria>();
    public DbSet<MovimentoBancario> MovimentosBancarios => Set<MovimentoBancario>();
    public DbSet<CategoriaContabil> CategoriaContabeis => Set<CategoriaContabil>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Utilizador>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Telefone).IsUnique();
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.Property(x => x.Telefone).HasMaxLength(20).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Perfil).HasMaxLength(30).HasDefaultValue("Operador");
            e.HasMany(x => x.Sessoes).WithOne(x => x.Utilizador).HasForeignKey(x => x.UtilizadorId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<SessaoActiva>().HasKey(x => x.Id);
        mb.Entity<SessaoActiva>().Property(x => x.Dispositivo).HasMaxLength(200);
        mb.Entity<SessaoActiva>().Property(x => x.Localizacao).HasMaxLength(200);

        mb.Entity<Empresa>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.NIF).HasMaxLength(30);
            e.Property(x => x.Morada).HasMaxLength(300);
            e.Property(x => x.Cidade).HasMaxLength(100);
            e.Property(x => x.Pais).HasMaxLength(100);
            e.Property(x => x.Telefone).HasMaxLength(30);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Website).HasMaxLength(200);
            e.Property(x => x.Capital).HasPrecision(18,2);
        });

        mb.Entity<Configuracao>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.LimiarSaldoBaixo).HasPrecision(18,2);
            e.Property(x => x.EmailNotificacoes).HasMaxLength(200);
        });

        mb.Entity<Webhook>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Evento).HasMaxLength(100).IsRequired();
            e.Property(x => x.Url).HasMaxLength(500).IsRequired();
        });

        mb.Entity<ContaContabil>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Codigo).IsUnique();
            e.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.Grupo).HasMaxLength(30).IsRequired();
        });

        mb.Entity<CategoriaContabil>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
            e.Property(x => x.ContaDebito).HasMaxLength(20).IsRequired();
            e.Property(x => x.ContaCredito).HasMaxLength(20).IsRequired();
            e.Property(x => x.GrupoDre).HasMaxLength(100);
            e.Property(x => x.GrupoBalanco).HasMaxLength(100);
            e.Property(x => x.GrupoFluxoCaixa).HasMaxLength(100);
        });

        mb.Entity<Lancamento>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Descricao).HasMaxLength(500).IsRequired();
            e.Property(x => x.Categoria).HasMaxLength(200);
            e.Property(x => x.Tipo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Valor).HasPrecision(18,2);
            e.Property(x => x.Beneficiario).HasMaxLength(100);
            e.Property(x => x.MetodoPagamento).HasMaxLength(50);
            e.Property(x => x.CaminhoDocumento).HasMaxLength(500);
            e.Property(x => x.Observacoes).HasMaxLength(500);
            e.Property(x => x.CentroCusto).HasMaxLength(100);
            e.Property(x => x.ReferenciaInterna).HasMaxLength(100);
            e.Property(x => x.ImpostoSelo).HasPrecision(18,2);
            e.Property(x => x.Anulado).HasDefaultValue(false);
            e.HasIndex(x => x.Data).HasDatabaseName("IX_Lancamento_Data");
            e.HasOne(x => x.ContaBancaria).WithMany(x => x.Lancamentos).HasForeignKey(x => x.ContaBancariaId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.CategoriaContabil).WithMany().HasForeignKey(x => x.CategoriaContabilId).OnDelete(DeleteBehavior.SetNull);
        });

        mb.Entity<LancamentoDetalhe>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Debito).HasPrecision(18,2);
            e.Property(x => x.Credito).HasPrecision(18,2);
            e.HasOne(x => x.Lancamento).WithMany(x => x.Detalhes).HasForeignKey(x => x.LancamentoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ContaContabil).WithMany(x => x.Lancamentos).HasForeignKey(x => x.ContaContabilId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<ContaBancaria>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Banco).HasMaxLength(100).IsRequired();
            e.Property(x => x.NIB).HasMaxLength(50);
            e.Property(x => x.Tipo).HasMaxLength(50);
            e.Property(x => x.Moeda).HasMaxLength(10);
            e.Property(x => x.SaldoAtual).HasPrecision(18,2);
            e.Property(x => x.SaldoOntem).HasPrecision(18,2);
            e.Property(x => x.Agencia).HasMaxLength(150);
            e.Property(x => x.Titular).HasMaxLength(200);
            e.HasIndex(x => x.NIB).IsUnique();
            e.HasOne(x => x.ContaContabil).WithMany().HasForeignKey(x => x.ContaContabilId).OnDelete(DeleteBehavior.SetNull);
        });

        mb.Entity<MovimentoBancario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Descricao).HasMaxLength(500).IsRequired();
            e.Property(x => x.Referencia).HasMaxLength(50);
            e.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Valor).HasPrecision(18,2);
            e.HasOne(x => x.ContaBancaria).WithMany(x => x.Movimentos).HasForeignKey(x => x.ContaBancariaId).OnDelete(DeleteBehavior.Cascade);
        });

        Seed(mb);
    }

    private static void Seed(ModelBuilder mb)
    {
        mb.Entity<Empresa>().HasData(new
        {
            Id=1, Nome="SmartGest, Lda.", NIF="5417000001",
            Morada="Luanda", Cidade="Luanda", Pais="Angola",
            Telefone="", Email="", Website="", Capital=10_000_000m
        });
        mb.Entity<Configuracao>().HasData(new
        {
            Id=1, TemaIndex=0, IdiomaIndex=0, MoedaIndex=0, DataFormatoIndex=0,
            MostrarSparklines=true, AnimacoesAtivadas=true, MostrarSaldosOcultos=false,
            NotifEmail=true, NotifApp=true, NotifSaldoBaixo=true, NotifLancamentos=true,
            NotifRelatorios=false, NotifErrosSistema=true, NotifBackup=true,
            EmailNotificacoes="", LimiarSaldoBaixo=500_000m,
            DoisFatoresAtivo=false, SessaoTimeoutMins=30, RegistarAuditoria=true
        });

        var categorias = new[]
        {
            new { Id=1, Nome="Venda de Produto", Tipo="Entrada", ContaDebito="45", ContaCredito="61", GrupoDre="Proveitos e Ganhos", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=2, Nome="Prestação de Serviços", Tipo="Entrada", ContaDebito="45", ContaCredito="62", GrupoDre="Proveitos e Ganhos", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=3, Nome="Recebimento de Cliente", Tipo="Entrada", ContaDebito="45", ContaCredito="31", GrupoDre="", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=4, Nome="Aporte de Capital", Tipo="Entrada", ContaDebito="45", ContaCredito="51", GrupoDre="", GrupoBalanco="CapitalProprio", GrupoFluxoCaixa="Financiamento", AplicaImpostoSelo=false, Ativo=true },
            new { Id=5, Nome="Empréstimo Recebido", Tipo="Entrada", ContaDebito="45", ContaCredito="33", GrupoDre="", GrupoBalanco="PassivoCorrente", GrupoFluxoCaixa="Financiamento", AplicaImpostoSelo=false, Ativo=true },
            new { Id=6, Nome="Juros Recebidos", Tipo="Entrada", ContaDebito="45", ContaCredito="66", GrupoDre="Proveitos e Ganhos", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=7, Nome="Reembolso", Tipo="Entrada", ContaDebito="45", ContaCredito="63", GrupoDre="", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=8, Nome="Transferência Recebida", Tipo="Entrada", ContaDebito="45", ContaCredito="45", GrupoDre="", GrupoBalanco="", GrupoFluxoCaixa="Financiamento", AplicaImpostoSelo=false, Ativo=true },
            new { Id=9, Nome="Comissão Recebida", Tipo="Entrada", ContaDebito="45", ContaCredito="62", GrupoDre="Proveitos e Ganhos", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=10, Nome="Outras Entradas", Tipo="Entrada", ContaDebito="45", ContaCredito="63", GrupoDre="Proveitos e Ganhos", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=12, Nome="Compra de Mercadoria", Tipo="Saída", ContaDebito="71", ContaCredito="45", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=13, Nome="Pagamento a Fornecedor", Tipo="Saída", ContaDebito="32", ContaCredito="45", GrupoDre="", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=14, Nome="Salários", Tipo="Saída", ContaDebito="73", ContaCredito="45", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=15, Nome="INSS", Tipo="Saída", ContaDebito="73", ContaCredito="36", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=16, Nome="IRT", Tipo="Saída", ContaDebito="75", ContaCredito="34", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=17, Nome="IVA", Tipo="Saída", ContaDebito="75", ContaCredito="34", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=18, Nome="Impostos e Taxas", Tipo="Saída", ContaDebito="75", ContaCredito="34", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=19, Nome="Despesa Administrativa", Tipo="Saída", ContaDebito="72", ContaCredito="45", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=20, Nome="Energia / Água / Internet", Tipo="Saída", ContaDebito="72", ContaCredito="45", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=21, Nome="Aluguer", Tipo="Saída", ContaDebito="72", ContaCredito="45", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=22, Nome="Combustível / Transportes", Tipo="Saída", ContaDebito="72", ContaCredito="45", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true },
            new { Id=23, Nome="Compra de Equipamento", Tipo="Saída", ContaDebito="11", ContaCredito="45", GrupoDre="", GrupoBalanco="AtivoNaoCorrente", GrupoFluxoCaixa="Investimento", AplicaImpostoSelo=false, Ativo=true },
            new { Id=24, Nome="Juros Bancários", Tipo="Saída", ContaDebito="78", ContaCredito="45", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Financiamento", AplicaImpostoSelo=false, Ativo=true },
            new { Id=25, Nome="Amortização de Empréstimo", Tipo="Saída", ContaDebito="33", ContaCredito="45", GrupoDre="", GrupoBalanco="", GrupoFluxoCaixa="Financiamento", AplicaImpostoSelo=false, Ativo=true },
            new { Id=26, Nome="Outras Despesas", Tipo="Saída", ContaDebito="76", ContaCredito="45", GrupoDre="Custos e Perdas", GrupoBalanco="", GrupoFluxoCaixa="Operacional", AplicaImpostoSelo=false, Ativo=true }
        };
        mb.Entity<CategoriaContabil>().HasData(categorias);
    }
}
