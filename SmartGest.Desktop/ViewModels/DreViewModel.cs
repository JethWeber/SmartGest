using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.ViewModels;

public partial class DreViewModel : ViewModelBase
{
    private readonly ContabilidadeService _svc;

    // ── Estado de carregamento ────────────────────────────────────────────────
    [ObservableProperty] private bool   _carregando   = false;
    [ObservableProperty] private string _erroMensagem = string.Empty;
    [ObservableProperty] private bool   _temErro      = false;

    // ── Métricas do Topo ──────────────────────────────────────────────────────
    [ObservableProperty] private string _totalReceitas     = "– Kzs";
    [ObservableProperty] private string _totalCustos       = "– Kzs";
    [ObservableProperty] private string _resultadoLiquido  = "– Kzs";
    [ObservableProperty] private string _receitasVariacao  = string.Empty;
    [ObservableProperty] private string _custosVariacao    = string.Empty;
    [ObservableProperty] private string _resultadoVariacao = string.Empty;
    [ObservableProperty] private string _corResultado      = "#43A047";

    // ── Sparklines ────────────────────────────────────────────────────────────
    public ISeries[] SparklineReceitas  { get; } = Sparklines(SKColors.MediumSeaGreen);
    public ISeries[] SparklineCustos    { get; } = Sparklines(SKColors.Tomato);
    public ISeries[] SparklineResultado { get; } = Sparklines(new SKColor(0x1A, 0x2E, 0x5A));

    // ── Gráfico de barras ─────────────────────────────────────────────────────
    [ObservableProperty] private ISeries[] _seriesGrafico = Array.Empty<ISeries>();
    public Axis[] EixoX { get; private set; } = Array.Empty<Axis>();
    public Axis[] EixoY { get; private set; } = Array.Empty<Axis>();

    // ── Filtros ───────────────────────────────────────────────────────────────
    [ObservableProperty] private int             _filtroAnoIndex     = 0;
    [ObservableProperty] private int             _filtroPeriodoIndex = 0;
    [ObservableProperty] private DateTimeOffset? _filtroDataInicio   =
        new DateTimeOffset(DateTime.Today.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
    [ObservableProperty] private DateTimeOffset? _filtroDataFim = DateTimeOffset.Now;

    // ── Tabela ────────────────────────────────────────────────────────────────
    private ObservableCollection<DreLinhaItem> _todos = new();

    [ObservableProperty] private ObservableCollection<DreLinhaItem> _linhasFiltradas = new();
    [ObservableProperty] private string _totalLinhasTexto = string.Empty;

    // ── Totais do rodapé ──────────────────────────────────────────────────────
    [ObservableProperty] private string _rodapeReceitas     = "– Kzs";
    [ObservableProperty] private string _rodapeCustos       = "– Kzs";
    [ObservableProperty] private string _rodapeResultado    = "– Kzs";
    [ObservableProperty] private string _corRodapeResultado = "#43A047";

    // ─────────────────────────────────────────────────────────────────────────
    public DreViewModel(ContabilidadeService svc)
    {
        _svc = svc;

        var meses = new[] { "Jan","Fev","Mar","Abr","Mai","Jun",
                            "Jul","Ago","Set","Out","Nov","Dez" };

        SeriesGrafico = CriarSeriesGrafico(new double[12], new double[12]);

        EixoX = new Axis[]
        {
            new Axis
            {
                Labels      = meses,
                TextSize    = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB))
            }
        };
        EixoY = new Axis[]
        {
            new Axis
            {
                Labeler     = v => v >= 1000 ? $"{v / 1000:N0}k" : $"{v:N0}",
                TextSize    = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB)),
                MinLimit    = 0
            }
        };
    }

    private static ISeries[] CriarSeriesGrafico(double[] receitas, double[] custos) =>
        new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name   = "Receitas",
                Values = receitas,
                Fill   = new SolidColorPaint(new SKColor(0x43, 0xA0, 0x47)),
                Rx = 4, Ry = 4, MaxBarWidth = 28
            },
            new ColumnSeries<double>
            {
                Name   = "Custos",
                Values = custos,
                Fill   = new SolidColorPaint(new SKColor(0xE5, 0x39, 0x35)),
                Rx = 4, Ry = 4, MaxBarWidth = 28
            }
        };

    public DreViewModel() : this(App.Services.GetRequiredService<ContabilidadeService>()) { }

    public async Task InicializarAsync()
        => await CarregarAsync();

    // ── Comandos ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task Filtrar()
        => await CarregarAsync();

    [RelayCommand]
    private async Task Atualizar()
        => await CarregarAsync();

    [RelayCommand]
    private void Exportar()
    {
        // TODO: exportar para XLSX / PDF
    }

    // ── Lógica interna ────────────────────────────────────────────────────────

    private async Task CarregarAsync()
    {
        Carregando   = true;
        TemErro      = false;
        ErroMensagem = string.Empty;

        try
        {
            var inicio = ResolveFiltroInicio();
            var fim    = FiltroDataFim?.DateTime ?? DateTime.Today;

            var resp = await _svc.ObterDreAsync(inicio, fim);

            if (resp is null)
            {
                MostrarErro("Sem resposta do servidor.");
                return;
            }

            _todos = new ObservableCollection<DreLinhaItem>(
                resp.Linhas
                    .Where(l => l.ValorRealizado != 0)
                    .Select(l => new DreLinhaItem(
                        l.Codigo, l.Descricao, l.Grupo,
                        (double)l.ValorOrcado, (double)l.ValorRealizado,
                        l.IsReceita, l.DataOrigem)));

            LinhasFiltradas  = new ObservableCollection<DreLinhaItem>(_todos);
            TotalLinhasTexto = $"{LinhasFiltradas.Count} rubrica(s)";

            var fluxo  = resp.FluxoMensal ?? new List<DreFluxoMensalItem>();
            var recMes = fluxo.Select(f => (double)f.Receita).ToArray();
            var cusMes = fluxo.Select(f => (double)f.Despesa).ToArray();
            if (recMes.Length == 0) recMes = new double[12];
            if (cusMes.Length == 0) cusMes = new double[12];
            SeriesGrafico = CriarSeriesGrafico(recMes, cusMes);

            var rec = (double)resp.TotalReceitas;
            var cus = (double)resp.TotalCustos;
            var res = (double)resp.ResultadoLiquido;

            RodapeReceitas      = $"{rec:N0} Kzs";
            RodapeCustos        = $"{cus:N0} Kzs";
            RodapeResultado     = res >= 0 ? $"+{res:N0} Kzs" : $"-{Math.Abs(res):N0} Kzs";
            CorRodapeResultado  = res >= 0 ? "#43A047" : "#E53935";

            TotalReceitas    = RodapeReceitas;
            TotalCustos      = RodapeCustos;
            ResultadoLiquido = RodapeResultado;
            CorResultado     = CorRodapeResultado;

            ReceitasVariacao  = string.Empty;
            CustosVariacao    = string.Empty;
            ResultadoVariacao = string.Empty;
        }
        catch (ApiException ex)
        {
            MostrarErro($"Erro da API ({(int)ex.StatusCode}): {ex.Message}");
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro inesperado: {ex.Message}");
        }
        finally
        {
            Carregando = false;
        }
    }

    private DateTime ResolveFiltroInicio()
    {
        var hoje = DateTime.Today;
        return FiltroPeriodoIndex switch
        {
            1 => new DateTime(hoje.Year, hoje.Month <= 6 ? 1 : 7, 1),
            2 => new DateTime(hoje.Year, ((hoje.Month - 1) / 3) * 3 + 1, 1),
            3 => new DateTime(hoje.Year, hoje.Month, 1),
            _ => FiltroDataInicio?.DateTime ?? new DateTime(hoje.Year, 1, 1)
        };
    }

    private void MostrarErro(string msg)
    {
        ErroMensagem = msg;
        TemErro      = true;
    }

    private static ISeries[] Sparklines(SKColor cor) =>
        new ISeries[]
        {
            new LineSeries<double>
            {
                Values         = new double[] { 0, 0, 0, 0, 0, 0, 0, 0 },
                Stroke         = new SolidColorPaint(cor) { StrokeThickness = 2 },
                Fill           = null,
                GeometrySize   = 0,
                LineSmoothness = 1
            }
        };
}

// ── Record de linha da DRE ────────────────────────────────────────────────────
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

    // FIX #9: quando orçado = 0 (sem módulo de orçamento), mostrar "—" em vez
    // de valores fictícios calculados a partir do realizado
    public string ValorOrcadoFmt => ValorOrcado == 0
        ? "—"
        : $"{ValorOrcado:N0} Kzs";

    public string ValorRealizadoFmt => IsReceita
        ? $"+{ValorRealizado:N0} Kzs"
        : $"-{ValorRealizado:N0} Kzs";

    public double Desvio    => ValorRealizado - ValorOrcado;

    public string DesvioFmt => ValorOrcado == 0
        ? "—"
        : Desvio >= 0
            ? $"+{Desvio:N0} Kzs"
            : $"-{Math.Abs(Desvio):N0} Kzs";

    public string CorDesvio => ValorOrcado == 0
        ? "#9AA0AB"
        : Desvio >= 0 ? "#43A047" : "#E53935";

    public double ExecucaoPerc => ValorOrcado == 0 ? 0 : ValorRealizado / ValorOrcado * 100;

    public string ExecucaoFmt => ValorOrcado == 0
        ? "—"
        : $"{ExecucaoPerc:N1}%";

    public string CorGrupo   => IsReceita ? "#2E7D32" : "#C62828";
    public string FundoGrupo => IsReceita ? "#E8F5E9"  : "#FFEBEE";
    public string CorValor   => IsReceita ? "#43A047" : "#E53935";
}
