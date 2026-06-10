using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly DashboardService _svc;

    // ── Métricas ─────────────────────────────────────────────────────────────
    [ObservableProperty] private string _totalReceita    = "— Kzs";
    [ObservableProperty] private string _totalDespesa    = "— Kzs";
    [ObservableProperty] private string _lucroLiquido    = "— Kzs";
    [ObservableProperty] private string _receitaVariacao = string.Empty;
    [ObservableProperty] private string _despesaVariacao   = string.Empty;
    [ObservableProperty] private string _lucroVariacao    = string.Empty;
    [ObservableProperty] private string _corReceitaVar     = "#43A047";
    [ObservableProperty] private string _corDespesaVar    = "#43A047";
    [ObservableProperty] private string _corLucroVar       = "#43A047";
    [ObservableProperty] private string _fundoReceitaVar  = "#E8F5E9";
    [ObservableProperty] private string _fundoDespesaVar = "#E8F5E9";
    [ObservableProperty] private string _fundoLucroVar    = "#E8F5E9";

    [ObservableProperty] private bool   _isLoading    = false;
    [ObservableProperty] private bool   _temErro      = false;
    [ObservableProperty] private string _erroMensagem = string.Empty;

    // ── Gráficos ─────────────────────────────────────────────────────────────
    [ObservableProperty] private ISeries[] _fluxoFinanceiroSeries = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _distribuicaoSeries   = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _sparklineReceita     = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _sparklineDespesa     = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _sparklineLucro       = Array.Empty<ISeries>();

    public Axis[] XAxes { get; private set; } = Array.Empty<Axis>();
    public Axis[] YAxes { get; private set; } = Array.Empty<Axis>();

    [ObservableProperty] private ObservableCollection<MovimentacaoItem> _ultimasMovimentacoes = new();

    public DashboardViewModel(DashboardService svc)
    {
        _svc = svc;
        InicializarEixos();
        _ = CarregarAsync();
    }

    public DashboardViewModel() : this(new DashboardService(new ApiClient(new TokenStore())))
    {
        TotalReceita = "1.500.000 Kzs";
        TotalDespesa = "600.000 Kzs";
        LucroLiquido = "900.000 Kzs";
    }

    public Task ActivarAsync() => CarregarAsync();

    private void InicializarEixos()
    {
        var meses = new[] { "Jan","Fev","Mar","Abr","Mai","Jun","Jul","Ago","Set","Out","Nov","Dez" };
        XAxes = new[] { new Axis { Labels = meses, TextSize = 11, LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB)), ShowSeparatorLines = false } };
        YAxes = new[] {
            new Axis
            {
                Labeler = v => $"{v:N0} Kzs",
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB)),
                ShowSeparatorLines = true,
                SeparatorsPaint = new SolidColorPaint(new SKColor(0xFF, 0xFF, 0xFF, 0x14))
            }
        };
    }

    private async Task CarregarAsync()
    {
        IsLoading    = true;
        TemErro      = false;
        ErroMensagem = string.Empty;

        try
        {
            var resp = await _svc.ObterAsync();
            if (resp is null)
            {
                MostrarErro("Não foi possível carregar o dashboard.");
                return;
            }

            TotalReceita = $"{resp.TotalReceita:N0} Kzs";
            TotalDespesa = $"{resp.TotalDespesa:N0} Kzs";
            LucroLiquido = $"{resp.LucroLiquido:N0} Kzs";

            AplicarVariacao(resp.TotalReceita, resp.ReceitaAnoAnterior,
                v => ReceitaVariacao = v, (c, f) => { CorReceitaVar = c; FundoReceitaVar = f; });
            AplicarVariacaoDespesa(resp.TotalDespesa, resp.DespesaAnoAnterior);
            AplicarVariacao(resp.LucroLiquido, resp.LucroAnoAnterior,
                v => LucroVariacao = v, (c, f) => { CorLucroVar = c; FundoLucroVar = f; });

            var recVals = resp.FluxoMensal.Select(m => (double)m.Receita).ToArray();
            var desVals = resp.FluxoMensal.Select(m => (double)m.Despesa).ToArray();
            var lucVals = resp.FluxoMensal.Select(m => (double)m.Lucro).ToArray();

            // Derivar uma série de Meta a partir do total anual (distribuição uniforme com ligeira subida mensal)
            var metaBase = (double)resp.TotalReceita / 12.0;
            var metaVals = Enumerable.Range(1, 12).Select(i => metaBase * (1.0 + 0.02 * (i - 1))).ToArray();

            FluxoFinanceiroSeries = new ISeries[]
            {
                CriarLinhaPremium("Receita", recVals),
                CriarMetaLinha("Meta", metaVals)
            };

            var total = (double)(resp.TotalReceita + resp.TotalDespesa + Math.Max(0, resp.LucroLiquido));
            if (total <= 0) total = 1;
            DistribuicaoSeries = new ISeries[]
            {
                new PieSeries<double> { Values = new[] { (double)resp.TotalReceita }, Name = "Receita", InnerRadius = 45, Fill = new SolidColorPaint(new SKColor(0x21, 0x96, 0xF3)) },
                new PieSeries<double> { Values = new[] { (double)resp.TotalDespesa }, Name = "Despesa", InnerRadius = 45, Fill = new SolidColorPaint(new SKColor(0xE5, 0x39, 0x35)) },
                new PieSeries<double> { Values = new[] { (double)Math.Max(0, resp.LucroLiquido) }, Name = "Lucro", InnerRadius = 45, Fill = new SolidColorPaint(new SKColor(0x43, 0xA0, 0x47)) }
            };

            SparklineReceita = Sparkline(Ultimos(recVals, 8), new SKColor(0x21, 0x96, 0xF3));
            SparklineDespesa = Sparkline(Ultimos(desVals, 8), new SKColor(0xE5, 0x39, 0x35));
            SparklineLucro   = Sparkline(Ultimos(lucVals, 8), new SKColor(0x43, 0xA0, 0x47));

            UltimasMovimentacoes = new ObservableCollection<MovimentacaoItem>(
                resp.UltimasMovimentacoes.Select(m => new MovimentacaoItem(
                    m.Data.ToString("dd/MM/yyyy"),
                    m.Tipo,
                    m.Categoria,
                    m.ContaBancariaNome ?? "—",
                    m.Tipo == "Entrada" ? $"+{m.Valor:N0} Kzs" : $"-{m.Valor:N0} Kzs",
                    m.Tipo == "Entrada")));
        }
        catch (ApiException ex)
        {
            MostrarErro($"Erro da API: {ex.Message}");
        }
        catch (HttpRequestException)
        {
            MostrarErro("Sem ligação à API. Verifique se o servidor está activo.");
        }
        catch (Exception ex)
        {
            MostrarErro(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static void AplicarVariacao(
        decimal atual, decimal anterior,
        Action<string> setTexto,
        Action<string, string> setCores)
    {
        var (texto, cor, fundo) = CalcularVariacao(atual, anterior, invertido: false);
        setTexto(texto);
        setCores(cor, fundo);
    }

    private void AplicarVariacaoDespesa(decimal atual, decimal anterior)
    {
        var (texto, cor, fundo) = CalcularVariacao(atual, anterior, invertido: true);
        DespesaVariacao = texto;
        CorDespesaVar   = cor;
        FundoDespesaVar = fundo;
    }

    private static (string Texto, string Cor, string Fundo) CalcularVariacao(
        decimal atual, decimal anterior, bool invertido)
    {
        if (anterior == 0 && atual == 0)
            return ("— vs ano passado", "#9AA0AB", "#F4F6FA");
        if (anterior == 0)
            return ("↑ Novo período", invertido ? "#E53935" : "#43A047", invertido ? "#FFEBEE" : "#E8F5E9");

        var pct = (double)((atual - anterior) / Math.Abs(anterior) * 100m);
        var subiu = pct >= 0;
        var positivo = invertido ? !subiu : subiu;
        var sinal = subiu ? "↑" : "↓";
        var cor   = positivo ? "#43A047" : "#E53935";
        var fundo = positivo ? "#E8F5E9" : "#FFEBEE";
        return ($"{sinal} {Math.Abs(pct):F1}% vs ano passado", cor, fundo);
    }

    private static double[] Ultimos(double[] vals, int n)
        => vals.Length <= n ? vals : vals[^n..];

    private static ISeries[] Sparkline(double[] values, SKColor cor) =>
        new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values.Length > 0 ? values : new[] { 0.0 },
                Stroke = new SolidColorPaint(cor) { StrokeThickness = 2 },
                Fill   = null, GeometrySize = 0, LineSmoothness = 0.8
            }
        };

    private static LineSeries<double> CriarLinha(string nome, double[] values, SKColor stroke, SKColor fill) =>
        new()
        {
            Name = nome, Values = values,
            Stroke = new SolidColorPaint(stroke) { StrokeThickness = 2.5f },
            Fill   = new SolidColorPaint(fill),
            GeometrySize = 5,
            GeometryStroke = new SolidColorPaint(stroke) { StrokeThickness = 2 },
            LineSmoothness = 0.55
        };

    private static LineSeries<double> CriarLinhaPremium(string nome, double[] values) =>
        new()
        {
            Name = nome,
            Values = values.Length > 0 ? values : new[] { 0.0 },
            Stroke = new SolidColorPaint(new SKColor(0x21, 0x96, 0xF3)) { StrokeThickness = 3f },
            // Approximate gradient with semi-transparent fill (Skia gradient not used here)
            Fill = new SolidColorPaint(new SKColor(0x21, 0x96, 0xF3, 0x59)),
            GeometrySize = 0,
            LineSmoothness = 0.8
        };

    private static LineSeries<double> CriarMetaLinha(string nome, double[] values) =>
        new()
        {
            Name = nome,
            Values = values.Length > 0 ? values : new[] { 0.0 },
            Stroke = new SolidColorPaint(new SKColor(0x90, 0xCA, 0xF9)) { StrokeThickness = 2f },
            Fill = null,
            GeometrySize = 4,
            GeometryStroke = new SolidColorPaint(new SKColor(0x90, 0xCA, 0xF9)) { StrokeThickness = 1f },
            LineSmoothness = 0.8
        };

    private void MostrarErro(string msg)
    {
        ErroMensagem = msg;
        TemErro      = true;
    }
}

public record MovimentacaoItem(
    string Data, string Tipo, string Categoria, string Conta, string Valor, bool IsEntrada)
{
    public string CorValor  => IsEntrada ? "#43A047" : "#E53935";
    public string FundoPill => IsEntrada ? "#E8F5E9"  : "#FFEBEE";
    public string CorPill   => IsEntrada ? "#43A047"  : "#E53935";
}
