using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace SmartGest.Desktop.ViewModels;

public partial class BalanceteViewModel : ViewModelBase
{
    // ── Métricas do Topo ──────────────────────────────────────────────────────
    [ObservableProperty] private string _totalDebitos   = "4.820.000 Kzs";
    [ObservableProperty] private string _totalCreditos  = "4.820.000 Kzs";
    [ObservableProperty] private string _diferencaSaldo = "0 Kzs";
    [ObservableProperty] private string _corDiferenca   = "#43A047";   // verde = balanceado

    // ── Sparklines dos Cards ──────────────────────────────────────────────────
    public ISeries[] SparklineDebitos  { get; }
    public ISeries[] SparklineCreditos { get; }
    public ISeries[] SparklineDif      { get; }

    // ── Filtros ───────────────────────────────────────────────────────────────
    [ObservableProperty] private string          _filtroTexto       = string.Empty;
    [ObservableProperty] private int             _filtroGrupoIndex  = 0;   // 0=Todos, 1=Ativo, 2=Passivo, 3=Capital, 4=Receita, 5=Despesa
    [ObservableProperty] private DateTimeOffset? _filtroDataInicio  = new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0, TimeSpan.Zero);
    [ObservableProperty] private DateTimeOffset? _filtroDataFim     = DateTimeOffset.Now;

    // ── Tabela ────────────────────────────────────────────────────────────────
    /// <summary>Todas as linhas de balancete (fonte de verdade).</summary>
    private readonly ObservableCollection<BalanceteItem> _todos;

    /// <summary>Subconjunto exibido na DataGrid após filtros.</summary>
    [ObservableProperty] private ObservableCollection<BalanceteItem> _contasFiltradas;

    [ObservableProperty] private string _totalContasTexto = string.Empty;

    // ─────────────────────────────────────────────────────────────────────────
    public BalanceteViewModel()
    {
        // ── 1. SPARKLINES ─────────────────────────────────────────────────────
        SparklineDebitos  = new ISeries[] { Sparkline(new double[] { 2, 4, 3.5, 5, 6, 7, 6.5, 8 }, new SKColor(0x21, 0x96, 0xF3)) };
        SparklineCreditos = new ISeries[] { Sparkline(new double[] { 2, 4, 3.5, 5, 6, 7, 6.5, 8 }, SKColors.MediumSeaGreen) };
        SparklineDif      = new ISeries[] { Sparkline(new double[] { 0, 0, 0,   0, 0, 0, 0,   0 }, new SKColor(0x1A, 0x2E, 0x5A)) };

        // ── 2. DADOS DE DEMONSTRAÇÃO ───────────────────────────────────────────
        _todos = new ObservableCollection<BalanceteItem>(GerarDemoData());
        _contasFiltradas = new ObservableCollection<BalanceteItem>(_todos);
        AtualizarTotais();
    }

    // ── Comandos ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void Filtrar()
    {
        var query = _todos.AsEnumerable();

        // Filtro de texto (código + nome)
        if (!string.IsNullOrWhiteSpace(FiltroTexto))
        {
            var termo = FiltroTexto.Trim().ToLower();
            query = query.Where(c =>
                c.Codigo.ToLower().Contains(termo) ||
                c.NomeConta.ToLower().Contains(termo) ||
                c.Grupo.ToLower().Contains(termo));
        }

        // Filtro de grupo
        query = FiltroGrupoIndex switch
        {
            1 => query.Where(c => c.Grupo == "Ativo"),
            2 => query.Where(c => c.Grupo == "Passivo"),
            3 => query.Where(c => c.Grupo == "Capital"),
            4 => query.Where(c => c.Grupo == "Receita"),
            5 => query.Where(c => c.Grupo == "Despesa"),
            _ => query
        };

        ContasFiltradas = new ObservableCollection<BalanceteItem>(query);
        AtualizarTotais();
    }

    [RelayCommand]
    private void Exportar()
    {
        // TODO: exportar para XLSX / PDF via repositório
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void AtualizarTotais()
    {
        var totalDeb = ContasFiltradas.Sum(c => c.MovDebito + c.SaldoAnteriorDebito);
        var totalCre = ContasFiltradas.Sum(c => c.MovCredito + c.SaldoAnteriorCredito);
        var dif      = totalDeb - totalCre;

        TotalDebitos   = $"{totalDeb:N0} Kzs";
        TotalCreditos  = $"{totalCre:N0} Kzs";
        DiferencaSaldo = $"{Math.Abs(dif):N0} Kzs";
        CorDiferenca   = dif == 0 ? "#43A047" : "#E53935";
        TotalContasTexto = $"{ContasFiltradas.Count} conta(s)";
    }

    private static LineSeries<double> Sparkline(double[] values, SKColor color) =>
        new LineSeries<double>
        {
            Values         = values,
            Stroke         = new SolidColorPaint(color) { StrokeThickness = 2 },
            Fill           = null,
            GeometrySize   = 0,
            LineSmoothness = 1
        };

    /// <summary>Gera um plano de contas de demonstração para o MVP.</summary>
    private static BalanceteItem[] GerarDemoData()
    {
        var rng = new Random(42);

        // (codigo, nome, grupo)
        var plano = new[]
        {
            ("11", "Caixa e Equivalentes",           "Ativo",   true),
            ("12", "Clientes a Receber",              "Ativo",   true),
            ("13", "Inventários",                     "Ativo",   true),
            ("14", "Imobilizações Corpóreas",         "Ativo",   true),
            ("15", "Imobilizações Incorpóreas",       "Ativo",   true),
            ("21", "Fornecedores a Pagar",            "Passivo", false),
            ("22", "Empréstimos Bancários",           "Passivo", false),
            ("23", "Encargos sobre Remunerações",     "Passivo", false),
            ("24", "Imposto a Pagar (IRT / IVA)",     "Passivo", false),
            ("31", "Capital Social",                  "Capital", false),
            ("32", "Reservas Legais",                 "Capital", false),
            ("33", "Resultados Transitados",          "Capital", false),
            ("71", "Vendas de Mercadorias",           "Receita", false),
            ("72", "Prestações de Serviços",          "Receita", false),
            ("73", "Outros Rendimentos Operacionais", "Receita", false),
            ("61", "Custo das Mercadorias Vendidas",  "Despesa", true),
            ("62", "Fornecimentos e Serviços Externos","Despesa",true),
            ("63", "Gastos com Pessoal",              "Despesa", true),
            ("64", "Amortizações e Depreciações",     "Despesa", true),
            ("65", "Encargos Financeiros",            "Despesa", true),
        };

        return plano.Select(p =>
        {
            double saldoAntDeb  = p.Item4 ? rng.Next(100, 900) * 10_000.0 : 0;
            double saldoAntCre  = p.Item4 ? 0 : rng.Next(100, 900) * 10_000.0;
            double movDeb       = rng.Next(50, 600) * 10_000.0;
            double movCre       = rng.Next(50, 600) * 10_000.0;

            double saldoFinalDeb = Math.Max(0, saldoAntDeb + movDeb - saldoAntCre - movCre);
            double saldoFinalCre = Math.Max(0, saldoAntCre + movCre - saldoAntDeb - movDeb);

            return new BalanceteItem(
                Codigo:              p.Item1,
                NomeConta:           p.Item2,
                Grupo:               p.Item3,
                SaldoAnteriorDebito: saldoAntDeb,
                SaldoAnteriorCredito:saldoAntCre,
                MovDebito:           movDeb,
                MovCredito:          movCre,
                SaldoFinalDebito:    saldoFinalDeb,
                SaldoFinalCredito:   saldoFinalCre
            );
        }).ToArray();
    }
}

// ── Record de linha do balancete ──────────────────────────────────────────────
public record BalanceteItem(
    string Codigo,
    string NomeConta,
    string Grupo,
    double SaldoAnteriorDebito,
    double SaldoAnteriorCredito,
    double MovDebito,
    double MovCredito,
    double SaldoFinalDebito,
    double SaldoFinalCredito)
{
    // Formatados para exibição (coluna vazia quando zero)
    public string SaldoAntDebFmt  => SaldoAnteriorDebito  > 0 ? $"{SaldoAnteriorDebito:N0}"  : "–";
    public string SaldoAntCreFmt  => SaldoAnteriorCredito > 0 ? $"{SaldoAnteriorCredito:N0}"  : "–";
    public string MovDebitoFmt    => MovDebito  > 0 ? $"{MovDebito:N0}"  : "–";
    public string MovCreditoFmt   => MovCredito > 0 ? $"{MovCredito:N0}" : "–";
    public string SaldoFinDebFmt  => SaldoFinalDebito  > 0 ? $"{SaldoFinalDebito:N0}"  : "–";
    public string SaldoFinCreFmt  => SaldoFinalCredito > 0 ? $"{SaldoFinalCredito:N0}" : "–";

    /// <summary>Cor da linha de grupo para badge.</summary>
    public string CorGrupo => Grupo switch
    {
        "Ativo"    => "#1565C0",
        "Passivo"  => "#E53935",
        "Capital"  => "#6A1B9A",
        "Receita"  => "#2E7D32",
        "Despesa"  => "#E65100",
        _          => "#555555"
    };

    public string FundoGrupo => Grupo switch
    {
        "Ativo"    => "#E3F2FD",
        "Passivo"  => "#FFEBEE",
        "Capital"  => "#F3E5F5",
        "Receita"  => "#E8F5E9",
        "Despesa"  => "#FFF3E0",
        _          => "#F4F6FA"
    };
}
