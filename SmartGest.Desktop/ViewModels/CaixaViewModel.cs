using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.ViewModels;

public partial class CaixaViewModel : ViewModelBase
{
    private readonly LancamentoService _lancamentoSvc;

    public event Action? OpenNovoLancamento;

    [ObservableProperty] private string _saldoCaixa       = "—"; 
    [ObservableProperty] private string _entradasDia      = "—";
    [ObservableProperty] private string _saidasDia        = "—";
    [ObservableProperty] private string _entradasVariacao = string.Empty;
    [ObservableProperty] private string _saidasVariacao   = string.Empty;
    [ObservableProperty] private string _saldoVariacao      = string.Empty;
    [ObservableProperty] private string _corEntradasVar    = "#43A047";
    [ObservableProperty] private string _corSaidasVar      = "#E53935";
    [ObservableProperty] private string _corSaldoVar       = "#1A2E5A";
    [ObservableProperty] private string _fundoEntradasVar  = "#E8F5E9";
    [ObservableProperty] private string _fundoSaidasVar    = "#FFEBEE";
    [ObservableProperty] private string _fundoSaldoVar     = "#E8EAF6";

    [ObservableProperty] private ISeries[] _sparklineSaldo    = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _sparklineEntradas = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _sparklineSaidas   = Array.Empty<ISeries>();

    [ObservableProperty] private string           _filtroTexto      = string.Empty;
    [ObservableProperty] private int              _filtroTipoIndex  = 0;
    [ObservableProperty] private DateTimeOffset?  _filtroDataInicio = DateTimeOffset.Now.AddDays(-30);
    [ObservableProperty] private DateTimeOffset?  _filtroDataFim    = DateTimeOffset.Now;

    [ObservableProperty] private ObservableCollection<LancamentoCaixaItem> _lancamentosFiltrados = new();
    [ObservableProperty] private string _totalLancamentosTexto = string.Empty;

    [ObservableProperty] private bool   _isLoading    = false;
    [ObservableProperty] private bool   _temErro      = false;
    [ObservableProperty] private string _erroMensagem = string.Empty;

    private int _totalRegistos = 0;
    private int _paginaAtual   = 1;
    private const int TamPagina = 50;

    public CaixaViewModel(LancamentoService lancamentoSvc)
    {
        _lancamentoSvc = lancamentoSvc;
        _ = CarregarAsync();
    }

    public CaixaViewModel()
    {
        var stubApi = new ApiClient(new TokenStore());
        _lancamentoSvc = new LancamentoService(stubApi);

        LancamentosFiltrados = new ObservableCollection<LancamentoCaixaItem>
        {
            new(1, "01/06/2026", "Entrada", "Venda de produto", "Venda de produto", "Banco BIC", 500_000m, 500_000m, new DateTime(2026,6,1)),
            new(2, "02/06/2026", "Saída",   "Compra de matéria-prima", "Despesas gerais", "Banco BAI", 200_000m, 300_000m, new DateTime(2026,6,2)),
        };
        TotalLancamentosTexto = "2 lançamento(s)";
        SaldoCaixa   = "300.000 Kzs";
        EntradasDia  = "500.000 Kzs";
        SaidasDia    = "200.000 Kzs";
        SparklineSaldo    = Sparkline(new[] { 100.0, 120, 90, 150, 130, 180, 300 }, new SKColor(0x1A, 0x2E, 0x5A));
        SparklineEntradas = Sparkline(new[] { 50.0, 80, 40, 120, 60, 90, 500 }, SKColors.MediumSeaGreen);
        SparklineSaidas   = Sparkline(new[] { 30.0, 40, 50, 70, 45, 60, 200 }, SKColors.Tomato);
    }

    private async Task CarregarAsync(int pagina = 1)
    {
        IsLoading    = true;
        TemErro      = false;
        ErroMensagem = string.Empty;

        try
        {
            var tipo = FiltroTipoIndex switch
            {
                1 => "Entrada",
                2 => "Saída",
                _ => (string?)null
            };

            var resp = await _lancamentoSvc.ListarAsync(
                tipo:       tipo,
                dataInicio: FiltroDataInicio?.DateTime,
                dataFim:    FiltroDataFim?.DateTime,
                texto:      string.IsNullOrWhiteSpace(FiltroTexto) ? null : FiltroTexto.Trim(),
                pagina:     pagina,
                tamPagina:  TamPagina);

            _totalRegistos = resp.Total;
            _paginaAtual   = pagina;

            var ordenados = resp.Items.OrderBy(l => l.Data).ToList();
            decimal saldoAcum = 0;
            var mapeados = ordenados.Select(l =>
            {
                saldoAcum += l.Tipo == "Entrada" ? l.Valor : -l.Valor;
                return new LancamentoCaixaItem(
                    l.Id, l.Data.ToString("dd/MM/yyyy"), l.Tipo, l.Descricao,
                    l.Categoria, l.ContaBancariaNome ?? "—",
                    l.Valor, saldoAcum, l.Data);
            }).OrderByDescending(l => l.DataOrigem).ToList();

            LancamentosFiltrados  = new ObservableCollection<LancamentoCaixaItem>(mapeados);
            TotalLancamentosTexto = $"{resp.Total} lançamento(s)";

            await ActualizarMetricasAsync();
        }
        catch (ApiException ex)
        {
            TemErro      = true;
            ErroMensagem = $"Erro da API ({(int)ex.StatusCode}): {ex.Message}";
        }
        catch (HttpRequestException)
        {
            TemErro      = true;
            ErroMensagem = "Sem ligação à API. Verifique se o servidor.";
        }
        catch (Exception ex)
        {
            TemErro      = true;
            ErroMensagem = $"Erro inesperado: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ActualizarMetricasAsync()
    {
        var fimFiltro = FiltroDataFim?.Date ?? DateTime.Today;
        var inicioSpark = fimFiltro.AddDays(-13);

        var spark = await _lancamentoSvc.ListarAsync(
            dataInicio: inicioSpark,
            dataFim:    fimFiltro,
            tamPagina:  500);

        var items = spark.Items;
        if (!items.Any())
        {
            SaldoCaixa      = "0 Kzs";
            EntradasDia     = "0 Kzs";
            SaidasDia       = "0 Kzs";
            SaldoVariacao   = "—";
            EntradasVariacao = "—";
            SaidasVariacao  = "—";
            CorSaldoVar     = "#9AA0AB";
            CorEntradasVar  = "#9AA0AB";
            CorSaidasVar    = "#9AA0AB";
            FundoSaldoVar   = "#F4F6FA";
            FundoEntradasVar = "#F4F6FA";
            FundoSaidasVar  = "#F4F6FA";
            SparklineEntradas = Sparkline(new[] { 0.0 }, SKColors.Gray);
            SparklineSaidas   = Sparkline(new[] { 0.0 }, SKColors.Gray);
            SparklineSaldo    = Sparkline(new[] { 0.0 }, SKColors.Gray);
            return;
        }

        var ultimoDia = items.Max(l => l.Data.Date);
        var ontem = ultimoDia.AddDays(-1);

        decimal SomaDia(DateTime dia, string tipo) =>
            items.Where(l => l.Data.Date == dia && l.Tipo == tipo).Sum(l => l.Valor);

        var entUltimo  = SomaDia(ultimoDia, "Entrada");
        var saiUltimo  = SomaDia(ultimoDia, "Saída");
        var entOntem   = SomaDia(ontem, "Entrada");
        var saiOntem   = SomaDia(ontem, "Saída");
        var saldoUltimo = entUltimo - saiUltimo;
        var saldoOntem  = entOntem - saiOntem;

        SaldoCaixa  = $"{saldoUltimo:N0} Kzs";
        EntradasDia = $"{entUltimo:N0} Kzs";
        SaidasDia   = $"{saiUltimo:N0} Kzs";

        AplicarVariacao(entUltimo, entOntem, false, (t, c, f) =>
        {
            EntradasVariacao = t; CorEntradasVar = c; FundoEntradasVar = f;
        });
        AplicarVariacao(saiUltimo, saiOntem, true, (t, c, f) =>
        {
            SaidasVariacao = t; CorSaidasVar = c; FundoSaidasVar = f;
        });
        AplicarVariacao(saldoUltimo, saldoOntem, false, (t, c, f) =>
        {
            SaldoVariacao = t; CorSaldoVar = c; FundoSaldoVar = f;
        });

        var dias = Enumerable.Range(0, 14).Select(i => inicioSpark.AddDays(i)).ToList();
        var entSerie = dias.Select(d => (double)SomaDia(d, "Entrada")).ToArray();
        var saiSerie = dias.Select(d => (double)SomaDia(d, "Saída")).ToArray();
        var salSerie = dias.Select(d => (double)(SomaDia(d, "Entrada") - SomaDia(d, "Saída"))).ToArray();

        SparklineEntradas = Sparkline(entSerie, new SKColor(0x43, 0xA0, 0x47));
        SparklineSaidas   = Sparkline(saiSerie, new SKColor(0xE5, 0x39, 0x35));
        SparklineSaldo    = Sparkline(salSerie, new SKColor(0x1A, 0x2E, 0x5A));
    }

    private static void AplicarVariacao(
        decimal atual, decimal anterior, bool invertido,
        Action<string, string, string> aplicar)
    {
        if (anterior == 0 && atual == 0)
        {
            aplicar("— vs ontem", "#9AA0AB", "#F4F6FA");
            return;
        }
        if (anterior == 0)
        {
            var cor = invertido ? "#E53935" : "#43A047";
            aplicar("↑ Novo", cor, invertido ? "#FFEBEE" : "#E8F5E9");
            return;
        }

        var pct = (double)((atual - anterior) / Math.Abs(anterior) * 100m);
        var subiu = pct >= 0;
        var positivo = invertido ? !subiu : subiu;
        var sinal = subiu ? "↑" : "↓";
        aplicar($"{sinal} {Math.Abs(pct):F0}% vs ontem",
            positivo ? "#43A047" : "#E53935",
            positivo ? "#E8F5E9" : "#FFEBEE");
    }

    [RelayCommand]
    private async Task FiltrarAsync() => await CarregarAsync(1);

    [RelayCommand]
    private async Task RefrescarAsync() => await CarregarAsync(_paginaAtual);

    [RelayCommand]
    private void NovaTransacao() => OpenNovoLancamento?.Invoke();

    public async Task OnLancamentoCriadoAsync() => await CarregarAsync(1);

    private static ISeries[] Sparkline(double[] values, SKColor color) =>
        new ISeries[]
        {
            new LineSeries<double>
            {
                Values         = values.Length > 0 ? values : new[] { 0.0 },
                Stroke         = new SolidColorPaint(color) { StrokeThickness = 2 },
                Fill           = new SolidColorPaint(color.WithAlpha(40)),
                GeometrySize   = 0,
                LineSmoothness = 0.7
            }
        };
}

public record LancamentoCaixaItem(
    int      Id,
    string   Data,
    string   Tipo,
    string   Descricao,
    string   Categoria,
    string   Conta,
    decimal  ValorBruto,
    decimal  SaldoAcum,
    DateTime DataOrigem)
{
    public bool   IsEntrada      => Tipo == "Entrada";
    public bool   IsSaida        => Tipo == "Saída";
    public string ValorFormatado => IsEntrada ? $"+{ValorBruto:N0} Kzs" : $"-{ValorBruto:N0} Kzs";
    public string SaldoAcumulado => $"{SaldoAcum:N0} Kzs";
    public string CorValor       => IsEntrada ? "#43A047" : "#E53935";
}
