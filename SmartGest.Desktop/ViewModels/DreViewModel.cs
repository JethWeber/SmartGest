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

    // ── Gráfico de barras (mantido estático por ora; alimentar com endpoint futuro) ──
    public ISeries[] SeriesGrafico { get; }
    public Axis[]    EixoX         { get; }
    public Axis[]    EixoY         { get; }

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
    [ObservableProperty] private string _rodapeReceitas      = "– Kzs";
    [ObservableProperty] private string _rodapeCustos        = "– Kzs";
    [ObservableProperty] private string _rodapeResultado     = "– Kzs";
    [ObservableProperty] private string _corRodapeResultado  = "#43A047";

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor via DI
    public DreViewModel(ContabilidadeService svc)
    {
        _svc = svc;

        // Gráfico de barras — construído uma vez; será actualizado quando
        // existir um endpoint de séries mensais.
        var meses = new[] { "Jan","Fev","Mar","Abr","Mai","Jun",
                            "Jul","Ago","Set","Out","Nov","Dez" };

        SeriesGrafico = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name   = "Receitas",
                Values = new double[12],
                Fill   = new SolidColorPaint(SKColors.MediumSeaGreen),
                Rx = 4, Ry = 4
            },
            new ColumnSeries<double>
            {
                Name   = "Custos",
                Values = new double[12],
                Fill   = new SolidColorPaint(SKColors.Tomato),
                Rx = 4, Ry = 4
            }
        };

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
                Labeler     = v => $"{v:N0}k",
                TextSize    = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB))
            }
        };
    }

    // Constructor sem parâmetros para design-time / code-behind
    public DreViewModel() : this(App.Services.GetRequiredService<ContabilidadeService>()) { }

    // ── Inicialização assíncrona chamada pela View ────────────────────────────
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

            // ── Preencher tabela ───────────────────────────────────────────────
            _todos = new ObservableCollection<DreLinhaItem>(
                resp.Linhas.Select(l => new DreLinhaItem(
                    l.Codigo, l.Nome, l.Grupo,
                    (double)l.ValorOrcado, (double)l.ValorRealizado,
                    l.IsReceita, l.DataOrigem)));

            LinhasFiltradas  = new ObservableCollection<DreLinhaItem>(_todos);
            TotalLinhasTexto = $"{LinhasFiltradas.Count} rubrica(s)";

            // ── Totais do rodapé e cards do topo ──────────────────────────────
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

            // Variações: não disponíveis via API actual — deixamos vazio
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

    /// <summary>
    /// Calcula a data de início a partir do filtro de período seleccionado,
    /// ou usa a data de início manual se o período for "Personalizado".
    /// </summary>
    private DateTime ResolveFiltroInicio()
    {
        var hoje = DateTime.Today;
        return FiltroPeriodoIndex switch
        {
            1 => new DateTime(hoje.Year, hoje.Month <= 6 ? 1 : 7, 1),           // Semestral
            2 => new DateTime(hoje.Year, ((hoje.Month - 1) / 3) * 3 + 1, 1),   // Trimestral
            3 => new DateTime(hoje.Year, hoje.Month, 1),                         // Mensal
            _ => FiltroDataInicio?.DateTime ?? new DateTime(hoje.Year, 1, 1)    // Anual / personalizado
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

// ── Record de linha da DRE (inalterado — usado pela View) ─────────────────────
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

    public double Desvio    => ValorRealizado - ValorOrcado;
    public string DesvioFmt => Desvio >= 0
        ? $"+{Desvio:N0} Kzs"
        : $"-{Math.Abs(Desvio):N0} Kzs";
    public string CorDesvio => Desvio >= 0 ? "#43A047" : "#E53935";

    public double ExecucaoPerc => ValorOrcado == 0 ? 0 : ValorRealizado / ValorOrcado * 100;
    public string ExecucaoFmt  => $"{ExecucaoPerc:N1}%";

    public string CorGrupo   => IsReceita ? "#2E7D32" : "#C62828";
    public string FundoGrupo => IsReceita ? "#E8F5E9"  : "#FFEBEE";
    public string CorValor   => IsReceita ? "#43A047" : "#E53935";
}
