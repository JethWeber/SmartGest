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

public partial class DreViewModel : ViewModelBase
{
    // ── Métricas do Topo ─────────────────────────────────────────────────────
    [ObservableProperty] private string _totalReceitas      = "4.820.000 Kzs";
    [ObservableProperty] private string _totalCustos        = "2.950.000 Kzs";
    [ObservableProperty] private string _resultadoLiquido   = "1.870.000 Kzs";
    [ObservableProperty] private string _receitasVariacao   = "+18%";
    [ObservableProperty] private string _custosVariacao     = "+7%";
    [ObservableProperty] private string _resultadoVariacao  = "+32%";
    [ObservableProperty] private string _corResultado       = "#43A047";

    // ── Sparklines dos Cards ──────────────────────────────────────────────────
    public ISeries[] SparklineReceitas  { get; }
    public ISeries[] SparklineCustos    { get; }
    public ISeries[] SparklineResultado { get; }

    // ── Gráfico de barras mensal ──────────────────────────────────────────────
    public ISeries[]  SeriesGrafico { get; }
    public Axis[]     EixoX         { get; }
    public Axis[]     EixoY         { get; }

    // ── Filtros ───────────────────────────────────────────────────────────────
    [ObservableProperty] private int              _filtroAnoIndex       = 0;
    [ObservableProperty] private int              _filtroPeriodoIndex   = 0;  // 0=Anual,1=Semestral,2=Trimestral,3=Mensal
    [ObservableProperty] private DateTimeOffset?  _filtroDataInicio     = new DateTimeOffset(DateTime.Today.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
    [ObservableProperty] private DateTimeOffset?  _filtroDataFim        = DateTimeOffset.Now;

    // ── Tabela ────────────────────────────────────────────────────────────────
    private readonly ObservableCollection<DreLinhaItem> _todos;

    [ObservableProperty] private ObservableCollection<DreLinhaItem> _linhasFiltradas;
    [ObservableProperty] private string _totalLinhasTexto = string.Empty;

    // ── Totais do rodapé ──────────────────────────────────────────────────────
    [ObservableProperty] private string _rodapeReceitas  = "4.820.000 Kzs";
    [ObservableProperty] private string _rodapeCustos    = "2.950.000 Kzs";
    [ObservableProperty] private string _rodapeResultado = "1.870.000 Kzs";
    [ObservableProperty] private string _corRodapeResultado = "#43A047";

    // ─────────────────────────────────────────────────────────────────────────
    public DreViewModel()
    {
        // ── 1. SPARKLINES ─────────────────────────────────────────────────────
        SparklineReceitas  = new ISeries[] { Sparkline(new double[] { 3, 4, 3.5, 5, 6, 5.5, 7, 8 },  SKColors.MediumSeaGreen) };
        SparklineCustos    = new ISeries[] { Sparkline(new double[] { 2, 3, 2.5, 4, 3.5, 4, 3,  5 }, SKColors.Tomato) };
        SparklineResultado = new ISeries[] { Sparkline(new double[] { 1, 1, 1,   1, 2.5, 1.5, 4, 3}, new SKColor(0x1A, 0x2E, 0x5A)) };

        // ── 2. GRÁFICO DE BARRAS ──────────────────────────────────────────────
        var meses = new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun",
                            "Jul", "Ago", "Set", "Out", "Nov", "Dez" };
        var receitas  = new double[] { 320, 410, 380, 450, 490, 420, 510, 480, 530, 460, 500, 590 };
        var custos    = new double[] { 210, 260, 240, 280, 300, 260, 310, 290, 320, 280, 300, 350 };

        SeriesGrafico = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name   = "Receitas",
                Values = receitas,
                Fill   = new SolidColorPaint(SKColors.MediumSeaGreen),
                Rx = 4, Ry = 4
            },
            new ColumnSeries<double>
            {
                Name   = "Custos",
                Values = custos,
                Fill   = new SolidColorPaint(SKColors.Tomato),
                Rx = 4, Ry = 4
            }
        };

        EixoX = new Axis[]
        {
            new Axis
            {
                Labels     = meses,
                TextSize   = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB))
            }
        };
        EixoY = new Axis[]
        {
            new Axis
            {
                Labeler    = v => $"{v:N0}k",
                TextSize   = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB))
            }
        };

        // ── 3. DADOS DE DEMONSTRAÇÃO ───────────────────────────────────────────
        _todos = new ObservableCollection<DreLinhaItem>(GerarDemoData());
        _linhasFiltradas = new ObservableCollection<DreLinhaItem>(_todos);
        AtualizarContador();
        RecalcularRodape();
    }

    // ── Comandos ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void Filtrar()
    {
        var query = _todos.AsEnumerable();

        if (FiltroDataInicio.HasValue)
        {
            var inicio = FiltroDataInicio.Value.Date;
            query = query.Where(l => l.DataOrigem >= inicio);
        }
        if (FiltroDataFim.HasValue)
        {
            var fim = FiltroDataFim.Value.Date;
            query = query.Where(l => l.DataOrigem <= fim);
        }

        // Filtro de período (simplificado para demo)
        if (FiltroPeriodoIndex == 3) // Mensal — apenas mês atual
        {
            var hoje = DateTime.Today;
            query = query.Where(l => l.DataOrigem.Year == hoje.Year && l.DataOrigem.Month == hoje.Month);
        }

        LinhasFiltradas = new ObservableCollection<DreLinhaItem>(query);
        AtualizarContador();
        RecalcularRodape();
    }

    [RelayCommand]
    private void Exportar()
    {
        // Ponto de extensão: integrar exportação para Excel / PDF
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void AtualizarContador()
        => TotalLinhasTexto = $"{LinhasFiltradas.Count} rubrica(s)";

    private void RecalcularRodape()
    {
        var rec  = LinhasFiltradas.Where(l => l.IsReceita).Sum(l => l.ValorBruto);
        var cus  = LinhasFiltradas.Where(l => !l.IsReceita).Sum(l => l.ValorBruto);
        var res  = rec - cus;

        RodapeReceitas  = $"{rec:N0} Kzs";
        RodapeCustos    = $"{cus:N0} Kzs";
        RodapeResultado = res >= 0 ? $"+{res:N0} Kzs" : $"-{Math.Abs(res):N0} Kzs";
        CorRodapeResultado = res >= 0 ? "#43A047" : "#E53935";

        // Actualiza cards do topo
        TotalReceitas    = RodapeReceitas;
        TotalCustos      = RodapeCustos;
        ResultadoLiquido = RodapeResultado;
        CorResultado     = CorRodapeResultado;
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

    private static DreLinhaItem[] GerarDemoData()
    {
        var rng  = new Random(7);
        var hoje = DateTime.Today;

        // Receitas
        var receitas = new[]
        {
            ("71", "Vendas de mercadorias",              "Receitas Operacionais",  true ),
            ("72", "Prestação de serviços",              "Receitas Operacionais",  true ),
            ("73", "Outros rendimentos operacionais",    "Receitas Operacionais",  true ),
            ("78", "Proveitos e ganhos financeiros",     "Receitas Financeiras",   true ),
            ("79", "Proveitos extraordinários",          "Receitas Extraordinárias", true),
        };

        // Custos
        var custos = new[]
        {
            ("61", "Custo das mercadorias vendidas",     "Custos Operacionais",    false),
            ("62", "Fornecimentos e serviços externos",  "Custos Operacionais",    false),
            ("63", "Despesas com pessoal",               "Custos Operacionais",    false),
            ("64", "Amortizações e depreciações",        "Custos Operacionais",    false),
            ("65", "Impostos e taxas",                   "Custos Operacionais",    false),
            ("66", "Outros custos operacionais",         "Custos Operacionais",    false),
            ("68", "Custos e perdas financeiras",        "Custos Financeiros",     false),
            ("69", "Custos extraordinários",             "Custos Extraordinários", false),
        };

        var lista = new System.Collections.Generic.List<DreLinhaItem>();

        double acumRec = 0, acumCus = 0;
        foreach (var (cod, desc, grupo, isRec) in receitas)
        {
            double v = rng.Next(200, 1200) * 1000.0;
            acumRec += v;
            lista.Add(new DreLinhaItem(
                Codigo: cod, Descricao: desc, Grupo: grupo,
                ValorOrcado: v * 0.9, ValorRealizado: v,
                IsReceita: true,
                DataOrigem: hoje.AddDays(-rng.Next(0, 90))
            ));
        }
        foreach (var (cod, desc, grupo, isRec) in custos)
        {
            double v = rng.Next(100, 800) * 1000.0;
            acumCus += v;
            lista.Add(new DreLinhaItem(
                Codigo: cod, Descricao: desc, Grupo: grupo,
                ValorOrcado: v * 1.05, ValorRealizado: v,
                IsReceita: false,
                DataOrigem: hoje.AddDays(-rng.Next(0, 90))
            ));
        }

        return lista.ToArray();
    }
}

// ── Record de linha da tabela ─────────────────────────────────────────────────
public record DreLinhaItem(
    string   Codigo,
    string   Descricao,
    string   Grupo,
    double   ValorOrcado,
    double   ValorRealizado,
    bool     IsReceita,
    DateTime DataOrigem)
{
    public double ValorBruto => ValorRealizado;

    public string ValorOrcadoFmt    => $"{ValorOrcado:N0} Kzs";
    public string ValorRealizadoFmt => IsReceita
        ? $"+{ValorRealizado:N0} Kzs"
        : $"-{ValorRealizado:N0} Kzs";

    public double Desvio     => ValorRealizado - ValorOrcado;
    public string DesvioFmt  => Desvio >= 0
        ? $"+{Desvio:N0} Kzs"
        : $"-{Math.Abs(Desvio):N0} Kzs";
    public string CorDesvio  => Desvio >= 0 ? "#43A047" : "#E53935";

    public double ExecucaoPerc => ValorOrcado == 0 ? 0 : ValorRealizado / ValorOrcado * 100;
    public string ExecucaoFmt  => $"{ExecucaoPerc:N1}%";

    // Cor e fundo da badge de grupo
    public string CorGrupo   => IsReceita ? "#2E7D32" : "#C62828";
    public string FundoGrupo => IsReceita ? "#E8F5E9"  : "#FFEBEE";

    // Cor do valor realizado
    public string CorValor => IsReceita ? "#43A047" : "#E53935";
}
