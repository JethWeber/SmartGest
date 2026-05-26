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

        mb.Entity<Lancamento>(e =>
        {
            e.Property(l => l.Valor).HasColumnType("decimal(18,2)");
            e.HasOne(l => l.ContaBancaria)
             .WithMany(cb => cb.Lancamentos)
             .HasForeignKey(l => l.ContaBancariaId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        mb.Entity<LancamentoDetalhe>(e =>
        {
            e.Property(d => d.Debito).HasColumnType("decimal(18,2)");
            e.Property(d => d.Credito).HasColumnType("decimal(18,2)");
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
            e.Property(c => c.SaldoAtual).HasColumnType("decimal(18,2)");
            e.Property(c => c.SaldoOntem).HasColumnType("decimal(18,2)");
            e.HasIndex(c => c.NIB).IsUnique();
        });

        mb.Entity<MovimentoBancario>(e =>
        {
            e.Property(m => m.Valor).HasColumnType("decimal(18,2)");
            e.HasOne(m => m.ContaBancaria)
             .WithMany(cb => cb.Movimentos)
             .HasForeignKey(m => m.ContaBancariaId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<Empresa>(e =>
        {
            e.Property(em => em.Capital).HasColumnType("decimal(18,2)");
        });

        mb.Entity<Configuracao>(e =>
        {
            e.Property(c => c.LimiarSaldoBaixo).HasColumnType("decimal(18,2)");
        });

        SeedData(mb);
    }

    private static void SeedData(ModelBuilder mb)
    {
        mb.Entity<Empresa>().HasData(new Empresa
        {
            Id = 1, Nome = "SmartGest, Lda.", NIF = "5417000001",
            Morada = "Rua da Missão, 42 · Luanda Sul", Cidade = "Luanda",
            Pais = "Angola", Telefone = "+244 923 000 000",
            Email = "geral@smartgest.ao", Website = "www.smartgest.ao",
            Capital = 10_000_000
        });

        mb.Entity<Configuracao>().HasData(new Configuracao { Id = 1 });

        mb.Entity<ContaContabil>().HasData(
            new ContaContabil { Id = 1,  Codigo = "11", Nome = "Caixa e Equivalentes de Caixa",          Grupo = "Ativo",   IsDevedora = true  },
            new ContaContabil { Id = 2,  Codigo = "12", Nome = "Clientes e Outras Contas a Receber",     Grupo = "Ativo",   IsDevedora = true  },
            new ContaContabil { Id = 3,  Codigo = "13", Nome = "Inventários e Activos Biológicos",       Grupo = "Ativo",   IsDevedora = true  },
            new ContaContabil { Id = 4,  Codigo = "14", Nome = "Activos Fixos Tangíveis",                Grupo = "Ativo",   IsDevedora = true  },
            new ContaContabil { Id = 5,  Codigo = "15", Nome = "Activos Intangíveis",                    Grupo = "Ativo",   IsDevedora = true  },
            new ContaContabil { Id = 6,  Codigo = "21", Nome = "Fornecedores e Contas a Pagar",          Grupo = "Passivo", IsDevedora = false },
            new ContaContabil { Id = 7,  Codigo = "22", Nome = "Empréstimos Bancários",                  Grupo = "Passivo", IsDevedora = false },
            new ContaContabil { Id = 8,  Codigo = "23", Nome = "Encargos sobre Remunerações",            Grupo = "Passivo", IsDevedora = false },
            new ContaContabil { Id = 9,  Codigo = "24", Nome = "Imposto a Pagar (IRT / IVA)",            Grupo = "Passivo", IsDevedora = false },
            new ContaContabil { Id = 10, Codigo = "31", Nome = "Capital Social",                         Grupo = "Capital", IsDevedora = false },
            new ContaContabil { Id = 11, Codigo = "32", Nome = "Reservas Legais",                        Grupo = "Capital", IsDevedora = false },
            new ContaContabil { Id = 12, Codigo = "33", Nome = "Resultados Transitados",                 Grupo = "Capital", IsDevedora = false },
            new ContaContabil { Id = 13, Codigo = "71", Nome = "Vendas de Mercadorias",                  Grupo = "Receita", IsDevedora = false },
            new ContaContabil { Id = 14, Codigo = "72", Nome = "Prestações de Serviços",                 Grupo = "Receita", IsDevedora = false },
            new ContaContabil { Id = 15, Codigo = "73", Nome = "Outros Rendimentos Operacionais",        Grupo = "Receita", IsDevedora = false },
            new ContaContabil { Id = 16, Codigo = "78", Nome = "Proveitos e Ganhos Financeiros",         Grupo = "Receita", IsDevedora = false },
            new ContaContabil { Id = 17, Codigo = "79", Nome = "Proveitos Extraordinários",              Grupo = "Receita", IsDevedora = false },
            new ContaContabil { Id = 18, Codigo = "61", Nome = "Custo das Mercadorias Vendidas",         Grupo = "Despesa", IsDevedora = true  },
            new ContaContabil { Id = 19, Codigo = "62", Nome = "Fornecimentos e Serviços Externos",      Grupo = "Despesa", IsDevedora = true  },
            new ContaContabil { Id = 20, Codigo = "63", Nome = "Gastos com Pessoal",                     Grupo = "Despesa", IsDevedora = true  },
            new ContaContabil { Id = 21, Codigo = "64", Nome = "Amortizações e Depreciações",            Grupo = "Despesa", IsDevedora = true  },
            new ContaContabil { Id = 22, Codigo = "65", Nome = "Impostos e Taxas",                       Grupo = "Despesa", IsDevedora = true  },
            new ContaContabil { Id = 23, Codigo = "66", Nome = "Outros Custos Operacionais",             Grupo = "Despesa", IsDevedora = true  },
            new ContaContabil { Id = 24, Codigo = "68", Nome = "Custos e Perdas Financeiras",            Grupo = "Despesa", IsDevedora = true  },
            new ContaContabil { Id = 25, Codigo = "69", Nome = "Custos Extraordinários",                 Grupo = "Despesa", IsDevedora = true  }
        );

        mb.Entity<ContaBancaria>().HasData(
            new ContaBancaria { Id = 1, Banco = "Banco BIC", NIB = "AO06.0055.0000.1234.5678.9012.3", Tipo = "Conta à Ordem",    Moeda = "AOA", SaldoAtual = 4_820_000, SaldoOntem = 4_600_000, Agencia = "Luanda — Maianga",   Titular = "SmartGest, Lda.", CorAccent = "#1A2E5A" },
            new ContaBancaria { Id = 2, Banco = "Banco BAI", NIB = "AO06.0040.0000.9876.5432.1098.7", Tipo = "Conta à Ordem",    Moeda = "AOA", SaldoAtual = 3_150_000, SaldoOntem = 3_200_000, Agencia = "Luanda — Ingombota", Titular = "SmartGest, Lda.", CorAccent = "#0D47A1" },
            new ContaBancaria { Id = 3, Banco = "Banco BPC", NIB = "AO06.0038.0000.1111.2222.3333.4", Tipo = "Depósito a Prazo", Moeda = "AOA", SaldoAtual = 2_980_000, SaldoOntem = 2_980_000, Agencia = "Luanda — Samba",     Titular = "SmartGest, Lda.", CorAccent = "#1B5E20" },
            new ContaBancaria { Id = 4, Banco = "Banco ATL", NIB = "AO06.0006.0000.4444.5555.6666.7", Tipo = "Conta à Ordem",    Moeda = "USD", SaldoAtual = 1_530_000, SaldoOntem = 1_490_000, Agencia = "Luanda — Talatona",  Titular = "SmartGest, Lda.", CorAccent = "#E65100" }
        );

        mb.Entity<Utilizador>().HasData(new Utilizador
        {
            Id           = 1,
            Nome         = "Augusto Barbosa",
            Email        = "augusto@smartgest.ao",
            Telefone     = "900000000",
            PasswordHash = "$2a$11$W6k9CR/erDeuqTYNChrLiutX.MZ2kdzXydirBWQ82ihKjeJlSIw32",
            Perfil       = "Administrador",
            Activo       = true,
            Iniciais     = "AB",
            CorAvatar    = "#1A2E5A"
        });
    }
}
