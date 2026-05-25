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

public partial class BalancoViewModel : ViewModelBase
{
    // ── Período ───────────────────────────────────────────────────────────────
    [ObservableProperty] private int    _exercicio     = DateTime.Today.Year;
    [ObservableProperty] private int    _mesIndex      = DateTime.Today.Month - 1; // 0-based
    [ObservableProperty] private string _periodoTexto  = string.Empty;

    // ── Métricas do Topo ──────────────────────────────────────────────────────
    [ObservableProperty] private string _totalAtivoFmt          = string.Empty;
    [ObservableProperty] private string _totalPassivoCapitalFmt = string.Empty;
    [ObservableProperty] private string _resultadoExercicioFmt  = string.Empty;
    [ObservableProperty] private string _corResultado           = "#43A047";
    [ObservableProperty] private bool   _estaEquilibrado        = true;
    [ObservableProperty] private string _textoEquilibrio        = "Balanceado ✓";
    [ObservableProperty] private string _corEquilibrio          = "#43A047";
    [ObservableProperty] private string _fundoEquilibrio        = "#E8F5E9";

    // ── Sparklines ────────────────────────────────────────────────────────────
    public ISeries[] SparklineAtivo          { get; }
    public ISeries[] SparklinePassivoCapital { get; }
    public ISeries[] SparklineResultado      { get; }

    // ── ATIVO ─────────────────────────────────────────────────────────────────
    public ObservableCollection<BalancoLinhaItem> AtivoCorrentes    { get; } = new();
    public ObservableCollection<BalancoLinhaItem> AtivoNaoCorrentes { get; } = new();

    [ObservableProperty] private double _totalAtivoCorrente    = 0;
    [ObservableProperty] private double _totalAtivoNaoCorrente = 0;
    [ObservableProperty] private double _totalAtivo            = 0;

    public string TotalAtivoCorrenteFmt    => $"{TotalAtivoCorrente:N0} Kzs";
    public string TotalAtivoNaoCorrenteFmt => $"{TotalAtivoNaoCorrente:N0} Kzs";

    // ── PASSIVO ───────────────────────────────────────────────────────────────
    public ObservableCollection<BalancoLinhaItem> PassivosCorrentes    { get; } = new();
    public ObservableCollection<BalancoLinhaItem> PassivosNaoCorrentes { get; } = new();

    [ObservableProperty] private double _totalPassivoCorrente    = 0;
    [ObservableProperty] private double _totalPassivoNaoCorrente = 0;
    [ObservableProperty] private double _totalPassivo            = 0;

    public string TotalPassivoCorrenteFmt    => $"{TotalPassivoCorrente:N0} Kzs";
    public string TotalPassivoNaoCorrenteFmt => $"{TotalPassivoNaoCorrente:N0} Kzs";

    // ── CAPITAL PRÓPRIO ───────────────────────────────────────────────────────
    public ObservableCollection<BalancoLinhaItem> CapitalProprio { get; } = new();

    [ObservableProperty] private double _totalCapitalProprio = 0;

    public string TotalCapitalProprioFmt => $"{TotalCapitalProprio:N0} Kzs";

    // ── Total Passivo + Capital ───────────────────────────────────────────────
    [ObservableProperty] private double _totalPassivoMaisCapital = 0;

    // ─────────────────────────────────────────────────────────────────────────
    public BalancoViewModel()
    {
        SparklineAtivo          = new ISeries[] { Sparkline(new double[] { 4, 5, 5.5, 6, 7, 8, 7.5, 9 }, new SKColor(0x21, 0x96, 0xF3)) };
        SparklinePassivoCapital = new ISeries[] { Sparkline(new double[] { 4, 5, 5.5, 6, 7, 8, 7.5, 9 }, new SKColor(0x1A, 0x2E, 0x5A)) };
        SparklineResultado      = new ISeries[] { Sparkline(new double[] { 1, 2, 1.5, 3, 4, 4, 5, 6   }, SKColors.MediumSeaGreen) };

        CarregarDemoData();
        RecalcularTotais();
    }

    // ── Comandos ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void Atualizar()
    {
        CarregarDemoData();
        RecalcularTotais();
    }

    [RelayCommand]
    private void Exportar()
    {
        // TODO: exportar para XLSX / PDF
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void RecalcularTotais()
    {
        TotalAtivoCorrente    = AtivoCorrentes.Sum(x => x.Valor);
        TotalAtivoNaoCorrente = AtivoNaoCorrentes.Sum(x => x.Valor);
        TotalAtivo            = TotalAtivoCorrente + TotalAtivoNaoCorrente;

        TotalPassivoCorrente    = PassivosCorrentes.Sum(x => x.Valor);
        TotalPassivoNaoCorrente = PassivosNaoCorrentes.Sum(x => x.Valor);
        TotalPassivo            = TotalPassivoCorrente + TotalPassivoNaoCorrente;

        TotalCapitalProprio     = CapitalProprio.Sum(x => x.Valor);
        TotalPassivoMaisCapital = TotalPassivo + TotalCapitalProprio;

        double resultado = TotalAtivo - TotalPassivo - (TotalCapitalProprio - CapitalProprio
            .FirstOrDefault(x => x.Descricao.Contains("Resultado"))?.Valor ?? 0);

        TotalAtivoFmt          = $"{TotalAtivo:N0} Kzs";
        TotalPassivoCapitalFmt = $"{TotalPassivoMaisCapital:N0} Kzs";

        double resultadoExercicio = CapitalProprio
            .FirstOrDefault(x => x.Descricao.Contains("Resultado"))?.Valor ?? 0;
        ResultadoExercicioFmt = $"{resultadoExercicio:N0} Kzs";
        CorResultado          = resultadoExercicio >= 0 ? "#43A047" : "#E53935";

        bool equilibrado = Math.Abs(TotalAtivo - TotalPassivoMaisCapital) < 1;
        EstaEquilibrado  = equilibrado;
        TextoEquilibrio  = equilibrado ? "Balanceado ✓" : "Desequilibrado ✗";
        CorEquilibrio    = equilibrado ? "#2E7D32" : "#C62828";
        FundoEquilibrio  = equilibrado ? "#E8F5E9"  : "#FFEBEE";

        var meses = new[] { "Jan","Fev","Mar","Abr","Mai","Jun","Jul","Ago","Set","Out","Nov","Dez" };
        PeriodoTexto = $"{meses[MesIndex]} {Exercicio}";

        OnPropertyChanged(nameof(TotalAtivoCorrenteFmt));
        OnPropertyChanged(nameof(TotalAtivoNaoCorrenteFmt));
        OnPropertyChanged(nameof(TotalPassivoCorrenteFmt));
        OnPropertyChanged(nameof(TotalPassivoNaoCorrenteFmt));
        OnPropertyChanged(nameof(TotalCapitalProprioFmt));
    }

    private void CarregarDemoData()
    {
        AtivoCorrentes.Clear();
        AtivoNaoCorrentes.Clear();
        PassivosCorrentes.Clear();
        PassivosNaoCorrentes.Clear();
        CapitalProprio.Clear();

        // ── ATIVO CORRENTE ────────────────────────────────────────────────────
        AtivoCorrentes.Add(new("Caixa e Equivalentes de Caixa",  2_340_000));
        AtivoCorrentes.Add(new("Clientes e Outras Contas a Receber", 1_850_000));
        AtivoCorrentes.Add(new("Adiantamentos a Fornecedores",      420_000));
        AtivoCorrentes.Add(new("Inventários e Activos Biológicos",   980_000));
        AtivoCorrentes.Add(new("Outros Activos Correntes",           210_000));

        // ── ATIVO NÃO CORRENTE ────────────────────────────────────────────────
        AtivoNaoCorrentes.Add(new("Activos Fixos Tangíveis (bruto)",    8_500_000));
        AtivoNaoCorrentes.Add(new("(–) Depreciações Acumuladas", -2_100_000, IsDeducao: true));
        AtivoNaoCorrentes.Add(new("Activos Intangíveis",                  650_000));
        AtivoNaoCorrentes.Add(new("Participações Financeiras",            300_000));
        AtivoNaoCorrentes.Add(new("Outros Activos Não Correntes",         180_000));

        // ── PASSIVO CORRENTE ──────────────────────────────────────────────────
        PassivosCorrentes.Add(new("Fornecedores e Contas a Pagar",    1_240_000));
        PassivosCorrentes.Add(new("Empréstimos Bancários CP",           600_000));
        PassivosCorrentes.Add(new("Imposto sobre o Rendimento (IRT)",   180_000));
        PassivosCorrentes.Add(new("Imposto sobre Valor Acrescentado",    95_000));
        PassivosCorrentes.Add(new("Outros Passivos Correntes",          320_000));

        // ── PASSIVO NÃO CORRENTE ──────────────────────────────────────────────
        PassivosNaoCorrentes.Add(new("Empréstimos Bancários LP",      2_500_000));
        PassivosNaoCorrentes.Add(new("Provisões para Riscos e Encargos", 380_000));
        PassivosNaoCorrentes.Add(new("Outros Passivos Não Correntes",    220_000));

        // ── CAPITAL PRÓPRIO ───────────────────────────────────────────────────
        CapitalProprio.Add(new("Capital Social",          3_000_000));
        CapitalProprio.Add(new("Reservas Legais",           450_000));
        CapitalProprio.Add(new("Reservas Livres",           320_000));
        CapitalProprio.Add(new("Resultados Transitados",    840_000));
        CapitalProprio.Add(new("Resultado do Exercício",    705_000));
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
}

// ── Linha de conta no balanço ─────────────────────────────────────────────────
public record BalancoLinhaItem(string Descricao, double Valor, bool IsDeducao = false)
{
    public string ValorFmt =>
        IsDeducao
            ? $"({Math.Abs(Valor):N0})"
            : $"{Valor:N0} Kzs";

    public string CorValor => IsDeducao ? "#E53935" : "#1A2E5A";
}
