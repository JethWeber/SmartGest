using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace SmartGest.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    // ── Métricas do Topo ─────────────────────────────────────────────────────
    [ObservableProperty] private string _totalReceita = "1.500.000 Kzs";
    [ObservableProperty] private string _totalDespesa = "600.000 Kzs";
    [ObservableProperty] private string _lucroLiquido = "900.000 Kzs";
    [ObservableProperty] private string _usuarioNome = "Augusto Barbosa";

    // ── Gráficos Principais ───────────────────────────────────────────────────
    public ISeries[] FluxoFinanceiroSeries { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }
    public ISeries[] DistribuicaoSeries { get; set; }

    // ── Sparklines dos Cards ──────────────────────────────────────────────────
    public ISeries[] SparklineReceita { get; set; }
    public ISeries[] SparklineDespesa { get; set; }
    public ISeries[] SparklineLucro   { get; set; }

    // ── Tabela ────────────────────────────────────────────────────────────────
    public ObservableCollection<MovimentacaoItem> UltimasMovimentacoes { get; set; }

    public DashboardViewModel()
    {
        // ── 1. FLUXO FINANCEIRO (3 linhas, 12 meses) ─────────────────────────
        FluxoFinanceiroSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = new double[] { 400000, 600000, 500000, 800000, 950000, 1200000,
                                        1050000, 1300000, 1100000, 1400000, 1350000, 1500000 },
                Name   = "Receita",
                Stroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 2 },
                Fill   = new LinearGradientPaint(
                    new[] { new SKColor(100, 149, 237, 60), SKColors.Transparent },
                    new SKPoint(0.5f, 0), new SKPoint(0.5f, 1)),
                GeometrySize   = 4,
                GeometryStroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 2 },
                LineSmoothness = 0.6
            },
            new LineSeries<double>
            {
                Values = new double[] { 200000, 350000, 280000, 420000, 390000, 500000,
                                        460000, 520000, 480000, 560000, 530000, 600000 },
                Name   = "Despesa",
                Stroke = new SolidColorPaint(SKColors.Tomato) { StrokeThickness = 2 },
                Fill   = new LinearGradientPaint(
                    new[] { new SKColor(255, 99, 71, 50), SKColors.Transparent },
                    new SKPoint(0.5f, 0), new SKPoint(0.5f, 1)),
                GeometrySize   = 4,
                GeometryStroke = new SolidColorPaint(SKColors.Tomato) { StrokeThickness = 2 },
                LineSmoothness = 0.6
            },
            new LineSeries<double>
            {
                Values = new double[] { 200000, 250000, 220000, 380000, 560000, 700000,
                                        590000, 780000, 620000, 840000, 820000, 900000 },
                Name   = "Lucro",
                Stroke = new SolidColorPaint(SKColors.MediumSeaGreen) { StrokeThickness = 2 },
                Fill   = new LinearGradientPaint(
                    new[] { new SKColor(60, 179, 113, 50), SKColors.Transparent },
                    new SKPoint(0.5f, 0), new SKPoint(0.5f, 1)),
                GeometrySize   = 4,
                GeometryStroke = new SolidColorPaint(SKColors.MediumSeaGreen) { StrokeThickness = 2 },
                LineSmoothness = 0.6
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Labels    = new[] { "Jan","Fev","Mar","Abr","Mai","Jun","Jul","Ago","Set","Out","Nov","Dez" },
                TextSize  = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(0x88, 0x88, 0x88))
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                Labeler     = value => $"{value / 1000:N0}k",
                TextSize    = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(0x88, 0x88, 0x88))
            }
        };

        // ── 2. DISTRIBUIÇÃO (rosca) ───────────────────────────────────────────
        DistribuicaoSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Values = new double[] { 60 }, Name = "Receita",
                Pushout = 2, InnerRadius = 45,
                Fill = new SolidColorPaint(SKColors.CornflowerBlue)
            },
            new PieSeries<double>
            {
                Values = new double[] { 25 }, Name = "Despesa",
                Pushout = 2, InnerRadius = 45,
                Fill = new SolidColorPaint(SKColors.Tomato)
            },
            new PieSeries<double>
            {
                Values = new double[] { 15 }, Name = "Lucro",
                Pushout = 2, InnerRadius = 45,
                Fill = new SolidColorPaint(SKColors.MediumSeaGreen)
            }
        };

        // ── 3. SPARKLINES ─────────────────────────────────────────────────────
        SparklineReceita = new ISeries[] { CreateSparkline(new double[] { 3, 5, 4, 7, 9, 8  }, SKColors.CornflowerBlue) };
        SparklineDespesa = new ISeries[] { CreateSparkline(new double[] { 2, 4, 3, 5, 4, 6  }, SKColors.Tomato) };
        SparklineLucro   = new ISeries[] { CreateSparkline(new double[] { 1, 1, 2, 3, 5, 4  }, SKColors.MediumSeaGreen) };

        // ── 4. TABELA ─────────────────────────────────────────────────────────
        UltimasMovimentacoes = new ObservableCollection<MovimentacaoItem>
        {
            new("05/06/2025", "Compra",  "Compra de matéria prima",  "ATL 021", "300.000 Kzs"),
            new("05/06/2025", "Venda",   "Venda de produto",          "BPC 01",  "200.000 Kzs"),
            new("05/06/2025", "Compra",  "Compra de matéria prima",  "ATL 021", "1.500.000 Kzs"),
            new("05/06/2025", "Compra",  "Compra de matéria prima",  "ATL 021", "1.500.000 Kzs"),
        };
    }

    private LineSeries<double> CreateSparkline(double[] values, SKColor color) =>
        new LineSeries<double>
        {
            Values         = values,
            Stroke         = new SolidColorPaint(color) { StrokeThickness = 2 },
            Fill           = null,
            GeometrySize   = 0,
            LineSmoothness = 1
        };
}

public record MovimentacaoItem(string Data, string Tipo, string Categoria, string Conta, string Valor);