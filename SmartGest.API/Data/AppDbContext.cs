using Microsoft.EntityFrameworkCore;
using SmartGest.API.Models;

namespace SmartGest.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Utilizador>        Utilizadores        => Set<Utilizador>();
    public DbSet<SessaoActiva>      Sessoes             => Set<SessaoActiva>();
    public DbSet<Empresa>           Empresas            => Set<Empresa>();
    public DbSet<Configuracao>      Configuracoes       => Set<Configuracao>();
    public DbSet<Webhook>           Webhooks            => Set<Webhook>();
    public DbSet<ContaContabil>     ContasContabeis     => Set<ContaContabil>();
    public DbSet<Lancamento>        Lancamentos         => Set<Lancamento>();
    public DbSet<LancamentoDetalhe> LancamentoDetalhes  => Set<LancamentoDetalhe>();
    public DbSet<ContaBancaria>     ContasBancarias     => Set<ContaBancaria>();
    public DbSet<MovimentoBancario> MovimentosBancarios => Set<MovimentoBancario>();
    public DbSet<CategoriaContabil> CategoriaContabeis  => Set<CategoriaContabil>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Utilizador>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Telefone).IsUnique();
            e.Property(u => u.Perfil).HasDefaultValue("Operador");
        });

        mb.Entity<SessaoActiva>(e =>
        {
            e.HasOne(s => s.Utilizador)
             .WithMany(u => u.Sessoes)
             .HasForeignKey(s => s.UtilizadorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<ContaContabil>(e =>
        {
            e.HasIndex(c => c.Codigo).IsUnique();
        });

        // Relação Lancamento -> CategoriaContabil (já existe, confirmar):
        mb.Entity<Lancamento>()
            .HasOne(l => l.CategoriaContabil)
            .WithMany()
            .HasForeignKey(l => l.CategoriaContabilId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relação ContaBancaria -> ContaContabil (nova — para resolução dinâmica do banco):
        mb.Entity<ContaBancaria>()
            .HasOne(b => b.ContaContabil)
            .WithMany()
            .HasForeignKey(b => b.ContaContabilId)
            .OnDelete(DeleteBehavior.SetNull);
        mb.Entity<Lancamento>(e =>
        {
            // PostgreSQL usa "numeric" para decimais de alta precisão
            e.Property(l => l.Valor).HasColumnType("numeric(18,2)");
            e.Property(l => l.ImpostoSelo).HasColumnType("numeric(18,2)");

            // Índice na Data para acelerar queries de relatórios por período
            e.HasIndex(l => l.Data).HasDatabaseName("IX_Lancamento_Data");

            // Soft Delete — campo para marcar como anulado sem apagar fisicamente
            e.Property(l => l.Anulado).HasDefaultValue(false);
            e.HasIndex(l => l.Anulado).HasDatabaseName("IX_Lancamento_Anulado");

            e.HasOne(l => l.ContaBancaria)
             .WithMany(cb => cb.Lancamentos)
             .HasForeignKey(l => l.ContaBancariaId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        mb.Entity<LancamentoDetalhe>(e =>
        {
            e.Property(d => d.Debito).HasColumnType("numeric(18,2)");
            e.Property(d => d.Credito).HasColumnType("numeric(18,2)");

            // Índice composto crítico para os SUMs dos relatórios no PostgreSQL
            e.HasIndex(d => new { d.ContaContabilId, d.Debito, d.Credito })
             .HasDatabaseName("IX_LancamentoDetalhe_ContaContabilId_Debito_Credito");

            e.HasOne(d => d.Lancamento)
             .WithMany(l => l.Detalhes)
             .HasForeignKey(d => d.LancamentoId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.ContaContabil)
             .WithMany(c => c.Lancamentos)
             .HasForeignKey(d => d.ContaContabilId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<ContaBancaria>(e =>
        {
            e.Property(c => c.SaldoAtual).HasColumnType("numeric(18,2)");
            e.Property(c => c.SaldoOntem).HasColumnType("numeric(18,2)");
            e.HasIndex(c => c.NIB).IsUnique();
        });

        mb.Entity<MovimentoBancario>(e =>
        {
            e.Property(m => m.Valor).HasColumnType("numeric(18,2)");
            e.HasOne(m => m.ContaBancaria)
             .WithMany(cb => cb.Movimentos)
             .HasForeignKey(m => m.ContaBancariaId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<Empresa>(e =>
        {
            e.Property(em => em.Capital).HasColumnType("numeric(18,2)");
        });

        mb.Entity<Configuracao>(e =>
        {
            e.Property(c => c.LimiarSaldoBaixo).HasColumnType("numeric(18,2)");
        });

        SeedData(mb);
    }

    private static void SeedData(ModelBuilder mb)
    {
        mb.Entity<Empresa>().HasData(new Empresa
        {
            Id      = 1,
            Nome    = "SmartGest, Lda.",
            NIF     = "5417000001",
            Morada  = "Rua da Missão, 42 · Luanda Sul",
            Cidade  = "Luanda",
            Pais    = "Angola",
            Telefone = "+244 923 000 000",
            Email   = "geral@smartgest.ao",
            Website = "www.smartgest.ao",
            Capital = 10_000_000
        });

        mb.Entity<Configuracao>().HasData(new Configuracao { Id = 1 });

        // ─────────────────────────────────────────────────────────────────────
        // PLANO GERAL DE CONTABILIDADE (PGC) DE ANGOLA — Decreto n.º 82/01
        //
        // Estrutura das 8 Classes:
        //   Classe 1 — Meios Fixos e Investimentos (Ativo Não Corrente)
        //   Classe 2 — Existências / Inventários (Ativo Corrente)
        //   Classe 3 — Terceiros (Clientes, Fornecedores, Estado, Pessoal)
        //   Classe 4 — Meios Monetários (Caixa e Bancos)
        //   Classe 5 — Capital e Reservas (Capital Próprio)
        //   Classe 6 — Custos e Perdas por Natureza (Gastos — Natureza Devedora)
        //   Classe 7 — Proveitos e Ganhos por Natureza (Receitas — Natureza Credora)
        //   Classe 8 — Resultados
        // ─────────────────────────────────────────────────────────────────────

        mb.Entity<ContaContabil>().HasData(
            new ContaContabil { Id = 1,  Codigo = "11", Nome = "Activos Fixos Tangíveis",                              Grupo = "Ativo",   IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 2,  Codigo = "12", Nome = "Activos Intangíveis",                                  Grupo = "Ativo",   IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 3,  Codigo = "13", Nome = "Investimentos Financeiros",                            Grupo = "Ativo",   IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 4,  Codigo = "18", Nome = "Amortizações Acumuladas",                              Grupo = "Ativo",   IsDevedora = false, Activa = true },
            new ContaContabil { Id = 5,  Codigo = "22", Nome = "Mercadorias",                                         Grupo = "Ativo",   IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 6,  Codigo = "26", Nome = "Matérias-Primas e Subsidiárias",                      Grupo = "Ativo",   IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 7,  Codigo = "31", Nome = "Clientes",                                            Grupo = "Ativo",   IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 8,  Codigo = "32", Nome = "Fornecedores",                                        Grupo = "Passivo", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 9,  Codigo = "33", Nome = "Empréstimos Obtidos",                                 Grupo = "Passivo", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 10, Codigo = "34", Nome = "Estado e Outros Entes Públicos",                      Grupo = "Passivo", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 11, Codigo = "36", Nome = "Pessoal",                                             Grupo = "Passivo", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 12, Codigo = "43", Nome = "Caixa",                                               Grupo = "Ativo",   IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 13, Codigo = "45", Nome = "Depósitos Bancários",                                 Grupo = "Ativo",   IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 14, Codigo = "51", Nome = "Capital Social",                                      Grupo = "Capital", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 15, Codigo = "55", Nome = "Reservas Legais",                                     Grupo = "Capital", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 16, Codigo = "59", Nome = "Resultados Transitados",                              Grupo = "Capital", IsDevedora = false, Activa = true },

            // ── Classe 7 = Custos e Perdas (PGC Angola, Decreto 82/01) ──────────────
            new ContaContabil { Id = 17, Codigo = "71", Nome = "Custo das Mercadorias Vendidas",                      Grupo = "Despesa", IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 18, Codigo = "72", Nome = "Fornecimentos e Serviços de Terceiros",               Grupo = "Despesa", IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 19, Codigo = "73", Nome = "Gastos com Pessoal",                                  Grupo = "Despesa", IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 20, Codigo = "74", Nome = "Amortizações e Depreciações do Exercício",            Grupo = "Despesa", IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 21, Codigo = "75", Nome = "Impostos e Taxas",                                    Grupo = "Despesa", IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 22, Codigo = "76", Nome = "Outros Custos e Perdas Operacionais",                 Grupo = "Despesa", IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 23, Codigo = "78", Nome = "Custos e Perdas Financeiras",                         Grupo = "Despesa", IsDevedora = true,  Activa = true },
            new ContaContabil { Id = 24, Codigo = "79", Nome = "Custos e Perdas Extraordinárias",                     Grupo = "Despesa", IsDevedora = true,  Activa = true },

            // ── Classe 6 = Proveitos e Ganhos (PGC Angola, Decreto 82/01) ───────────
            new ContaContabil { Id = 25, Codigo = "61", Nome = "Vendas de Mercadorias e Produtos Acabados",           Grupo = "Receita", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 26, Codigo = "62", Nome = "Prestações de Serviços",                              Grupo = "Receita", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 27, Codigo = "63", Nome = "Outros Proveitos e Ganhos Operacionais",              Grupo = "Receita", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 28, Codigo = "68", Nome = "Proveitos e Ganhos Financeiros",                      Grupo = "Receita", IsDevedora = false, Activa = true },
            new ContaContabil { Id = 29, Codigo = "69", Nome = "Proveitos e Ganhos Extraordinários",                  Grupo = "Receita", IsDevedora = false, Activa = true },

            new ContaContabil { Id = 30, Codigo = "88", Nome = "Resultado Líquido do Exercício",                      Grupo = "Capital", IsDevedora = false, Activa = true }
        );

        mb.Entity<CategoriaContabil>().HasData(
            // ENTRADAS
            new CategoriaContabil { Id = 1,  Nome = "Venda de Mercadoria",      Tipo = "Entrada", ContaDebito = "45", ContaCredito = "61", GrupoDre = "Proveitos e Ganhos", GrupoFluxoCaixa = "Operacional",   Ativo = true },
            new CategoriaContabil { Id = 2,  Nome = "Prestação de Serviço",     Tipo = "Entrada", ContaDebito = "45", ContaCredito = "62", GrupoDre = "Proveitos e Ganhos", GrupoFluxoCaixa = "Operacional",   Ativo = true },
            new CategoriaContabil { Id = 3,  Nome = "Recebimento de Cliente",   Tipo = "Entrada", ContaDebito = "45", ContaCredito = "31", GrupoDre = "",                   GrupoFluxoCaixa = "Operacional",   Ativo = true },
            new CategoriaContabil { Id = 4,  Nome = "Outros Rendimentos",       Tipo = "Entrada", ContaDebito = "45", ContaCredito = "63", GrupoDre = "Proveitos e Ganhos", GrupoFluxoCaixa = "Operacional",   Ativo = true },
            new CategoriaContabil { Id = 5,  Nome = "Capital Inicial",          Tipo = "Entrada", ContaDebito = "45", ContaCredito = "51", GrupoDre = "",                   GrupoFluxoCaixa = "Financiamento", Ativo = true },
            new CategoriaContabil { Id = 6,  Nome = "Empréstimo Bancário",      Tipo = "Entrada", ContaDebito = "45", ContaCredito = "33", GrupoDre = "",                   GrupoFluxoCaixa = "Financiamento", Ativo = true },
            new CategoriaContabil { Id = 7,  Nome = "Juros Recebidos",          Tipo = "Entrada", ContaDebito = "45", ContaCredito = "68", GrupoDre = "Proveitos e Ganhos", GrupoFluxoCaixa = "Financiamento", Ativo = true },
            new CategoriaContabil { Id = 8,  Nome = "Venda de Ativo",           Tipo = "Entrada", ContaDebito = "45", ContaCredito = "69", GrupoDre = "Proveitos e Ganhos", GrupoFluxoCaixa = "Investimento",  Ativo = true },
            new CategoriaContabil { Id = 9,  Nome = "Comissão Recebida",        Tipo = "Entrada", ContaDebito = "45", ContaCredito = "62", GrupoDre = "Proveitos e Ganhos", GrupoFluxoCaixa = "Operacional",   Ativo = true },
            new CategoriaContabil { Id = 10, Nome = "Outras Entradas",          Tipo = "Entrada", ContaDebito = "45", ContaCredito = "63", GrupoDre = "Proveitos e Ganhos", GrupoFluxoCaixa = "Operacional",   Ativo = true },
            // SAÍDAS
            new CategoriaContabil { Id = 11, Nome = "Compra de Mercadoria",        Tipo = "Saída", ContaDebito = "71", ContaCredito = "45", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 12, Nome = "Pagamento a Fornecedor",      Tipo = "Saída", ContaDebito = "32", ContaCredito = "45", GrupoDre = "",                GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 13, Nome = "Salários",                    Tipo = "Saída", ContaDebito = "73", ContaCredito = "45", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 14, Nome = "INSS",                        Tipo = "Saída", ContaDebito = "73", ContaCredito = "36", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 15, Nome = "IRT",                         Tipo = "Saída", ContaDebito = "75", ContaCredito = "34", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 16, Nome = "IVA",                         Tipo = "Saída", ContaDebito = "75", ContaCredito = "34", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 17, Nome = "Impostos e Taxas",            Tipo = "Saída", ContaDebito = "75", ContaCredito = "34", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 18, Nome = "Despesa Administrativa",      Tipo = "Saída", ContaDebito = "72", ContaCredito = "45", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 19, Nome = "Energia / Água / Internet",   Tipo = "Saída", ContaDebito = "72", ContaCredito = "45", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 20, Nome = "Aluguer",                     Tipo = "Saída", ContaDebito = "72", ContaCredito = "45", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 21, Nome = "Combustível / Transportes",   Tipo = "Saída", ContaDebito = "72", ContaCredito = "45", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true },
            new CategoriaContabil { Id = 22, Nome = "Compra de Equipamento",       Tipo = "Saída", ContaDebito = "11", ContaCredito = "45", GrupoDre = "",                GrupoFluxoCaixa = "Investimento",   Ativo = true },
            new CategoriaContabil { Id = 23, Nome = "Juros Bancários",             Tipo = "Saída", ContaDebito = "78", ContaCredito = "45", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Financiamento",  Ativo = true },
            new CategoriaContabil { Id = 24, Nome = "Amortização de Empréstimo",   Tipo = "Saída", ContaDebito = "33", ContaCredito = "45", GrupoDre = "",                GrupoFluxoCaixa = "Financiamento",  Ativo = true },
            new CategoriaContabil { Id = 25, Nome = "Outras Despesas",             Tipo = "Saída", ContaDebito = "76", ContaCredito = "45", GrupoDre = "Custos e Perdas", GrupoFluxoCaixa = "Operacional",    Ativo = true }
        );

        mb.Entity<Utilizador>().HasData(new Utilizador
        {
            Id           = 1,
            Nome         = "Jeth Weber",
            Email        = "weber@smartgest.ao",
            Telefone     = "900000000",
            PasswordHash = "$2b$11$a.TPqwNMTgPENS9Up.20f.CzgkVrSgpLt0KGfl3vWEWKeq/hEK7Rq",
            Perfil       = "Administrador",
            Activo       = true,
            Iniciais     = "JW",
            CorAvatar    = "#1A2E5A"
        }); // senha.: smartgest2026
    }
}
