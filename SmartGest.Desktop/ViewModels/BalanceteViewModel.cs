using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using QuestPDF.Infrastructure;
using SkiaSharp;
using SmartGest.Desktop.Ferramentas;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.ViewModels;

public partial class BalanceteViewModel : ViewModelBase
{
    private readonly ContabilidadeService _svc;
    private BalanceteApiResponse? _ultimoBalancete;

    public Window? OwnerWindow { get; set; }

    // ── Estado de carregamento ────────────────────────────────────────────────
    [ObservableProperty] private bool   _carregando   = false;
    [ObservableProperty] private string _erroMensagem = string.Empty;
    [ObservableProperty] private bool   _temErro      = false;

    // ── Métricas do Topo ──────────────────────────────────────────────────────
    [ObservableProperty] private string _totalDebitos   = "– Kzs";
    [ObservableProperty] private string _totalCreditos  = "– Kzs";
    [ObservableProperty] private string _diferencaSaldo = "– Kzs";
    [ObservableProperty] private string _corDiferenca   = "#43A047";

    // ── Sparklines dos Cards ──────────────────────────────────────────────────
    public ISeries[] SparklineDebitos  { get; } = Sparklines(new SKColor(0x21, 0x96, 0xF3));
    public ISeries[] SparklineCreditos { get; } = Sparklines(SKColors.MediumSeaGreen);
    public ISeries[] SparklineDif      { get; } = Sparklines(new SKColor(0x1A, 0x2E, 0x5A));

    // ── Filtros ───────────────────────────────────────────────────────────────
    [ObservableProperty] private string          _filtroTexto      = string.Empty;
    [ObservableProperty] private int             _filtroGrupoIndex = 0;
    [ObservableProperty] private DateTimeOffset? _filtroDataInicio =
        new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0, TimeSpan.Zero);
    [ObservableProperty] private DateTimeOffset? _filtroDataFim = DateTimeOffset.Now;

    // ── Tabela ────────────────────────────────────────────────────────────────
    private ObservableCollection<BalanceteItem> _todos = new();

    [ObservableProperty] private ObservableCollection<BalanceteItem> _contasFiltradas = new();
    [ObservableProperty] private string _totalContasTexto = string.Empty;

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor — injecção via DI
    public BalanceteViewModel(ContabilidadeService svc)
    {
        _svc = svc;
    }

    // Constructor sem parâmetros para o code-behind do Avalonia (design-time)
    public BalanceteViewModel() : this(App.Services.GetRequiredService<ContabilidadeService>()) { }

    // ── Inicialização assíncrona chamada pela View ─────────────────────────────
    public async Task InicializarAsync()
        => await CarregarAsync();

    // ── Comandos ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task Filtrar() => await CarregarAsync();

    [RelayCommand]
    private async Task Atualizar()
        => await CarregarAsync();

    [RelayCommand]
    private async Task Exportar()
    {
        if (_ultimoBalancete is null)
        {
            MostrarErro("Não há dados carregados para exportar.");
            return;
        }

        if (OwnerWindow is null)
            return;

        var topLevel = TopLevel.GetTopLevel(OwnerWindow);
        if (topLevel?.StorageProvider is null)
            return;

        var options = new FilePickerSaveOptions
        {
            Title = "Salvar Balancete como PDF",
            SuggestedFileName = "Balancete.pdf",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("PDF")
                {
                    Patterns = new[] { "*.pdf" },
                    MimeTypes = new[] { "application/pdf" }
                }
            }
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        if (file is null)
            return;

        try
        {
            BalancetePdfGenerator.Generate(_ultimoBalancete, file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            MostrarErro($"Erro ao gerar PDF: {ex.Message}");
        }
    }

    // ── Lógica interna ────────────────────────────────────────────────────────

    private async Task CarregarAsync()
    {
        Carregando  = true;
        TemErro     = false;
        ErroMensagem = string.Empty;

        try
        {
            var inicio = FiltroDataInicio?.DateTime;
            var fim    = FiltroDataFim?.DateTime;

            string? grupoFiltro = FiltroGrupoIndex switch
            {
                1 => "Ativo",
                2 => "Passivo",
                3 => "Capital",
                4 => "Receita",
                5 => "Despesa",
                _ => null
            };

            var resp = await _svc.ObterBalanceteAsync(inicio, fim, grupoFiltro);

            if (resp is null)
            {
                MostrarErro("Sem resposta do servidor.");
                return;
            }

            _ultimoBalancete = resp;

            // Actualizar métricas do topo
            var dif = resp.TotalDebitos - resp.TotalCreditos;
            TotalDebitos   = $"{resp.TotalDebitos:N0} Kzs";
            TotalCreditos  = $"{resp.TotalCreditos:N0} Kzs";
            DiferencaSaldo = $"{Math.Abs(dif):N0} Kzs";
            CorDiferenca   = dif == 0 ? "#43A047" : "#E53935";

            // Converter DTO → record de apresentação (só contas com movimento)
            _todos = new ObservableCollection<BalanceteItem>(
                resp.Items
                    .Where(i => TemMovimento(i))
                    .Select(i => new BalanceteItem(
                        i.Codigo, i.Nome, i.Grupo,
                        i.SaldoAnteriorDebito, i.SaldoAnteriorCredito,
                        i.MovDebito, i.MovCredito,
                        i.SaldoFinalDebito, i.SaldoFinalCredito)));

            AplicarFiltros();
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

    private void AplicarFiltros()
    {
        var query = _todos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(FiltroTexto))
        {
            var termo = FiltroTexto.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.Codigo.ToLowerInvariant().Contains(termo) ||
                c.NomeConta.ToLowerInvariant().Contains(termo) ||
                c.Grupo.ToLowerInvariant().Contains(termo));
        }

        // Filtro de grupo já foi aplicado na API; filtragem local é redundante
        // mas mantemos por segurança caso os dados vieram sem filtro de grupo.
        query = FiltroGrupoIndex switch
        {
            1 => query.Where(c => c.Grupo == "Ativo"),
            2 => query.Where(c => c.Grupo == "Passivo"),
            3 => query.Where(c => c.Grupo == "Capital"),
            4 => query.Where(c => c.Grupo == "Receita"),
            5 => query.Where(c => c.Grupo == "Despesa"),
            _ => query
        };

        ContasFiltradas  = new ObservableCollection<BalanceteItem>(query);
        TotalContasTexto = $"{ContasFiltradas.Count} conta(s)";
    }

    private static bool TemMovimento(BalanceteItemResponse i) =>
        i.SaldoAnteriorDebito  != 0 || i.SaldoAnteriorCredito != 0 ||
        i.MovDebito            != 0 || i.MovCredito           != 0 ||
        i.SaldoFinalDebito     != 0 || i.SaldoFinalCredito    != 0;

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

// ── Record de linha do balancete (inalterado — usado pela View) ───────────────
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
    public string SaldoAntDebFmt  => SaldoAnteriorDebito  > 0 ? $"{SaldoAnteriorDebito:N0}"  : "–";
    public string SaldoAntCreFmt  => SaldoAnteriorCredito > 0 ? $"{SaldoAnteriorCredito:N0}" : "–";
    public string MovDebitoFmt    => MovDebito  > 0 ? $"{MovDebito:N0}"  : "–";
    public string MovCreditoFmt   => MovCredito > 0 ? $"{MovCredito:N0}" : "–";
    public string SaldoFinDebFmt  => SaldoFinalDebito  > 0 ? $"{SaldoFinalDebito:N0}"  : "–";
    public string SaldoFinCreFmt  => SaldoFinalCredito > 0 ? $"{SaldoFinalCredito:N0}" : "–";
    public string DiferencaFmt
        => SaldoFinalDebito == SaldoFinalCredito
            ? "–"
            : (SaldoFinalDebito - SaldoFinalCredito) > 0
                ? $"+{Math.Abs(SaldoFinalDebito - SaldoFinalCredito):N0}"
                : $"-{Math.Abs(SaldoFinalDebito - SaldoFinalCredito):N0}";

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
