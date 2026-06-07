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
    // ── Dependências ──────────────────────────────────────────────────────────
    private readonly LancamentoService _lancamentoSvc;

    // ── Eventos ───────────────────────────────────────────────────────────────
    public event Action? OpenNovoLancamento;

    // ── Métricas do Topo ──────────────────────────────────────────────────────
    [ObservableProperty] private string _saldoCaixa       = "—";
    [ObservableProperty] private string _entradasDia      = "—";
    [ObservableProperty] private string _saidasDia        = "—";
    [ObservableProperty] private string _entradasVariacao = "";
    [ObservableProperty] private string _saidasVariacao   = "";

    // ── Sparklines ────────────────────────────────────────────────────────────
    public ISeries[] SparklineSaldo    { get; } = Sparkline(new double[] { 0 }, new SKColor(0x1A, 0x2E, 0x5A));
    public ISeries[] SparklineEntradas { get; } = Sparkline(new double[] { 0 }, SKColors.MediumSeaGreen);
    public ISeries[] SparklineSaidas   { get; } = Sparkline(new double[] { 0 }, SKColors.Tomato);

    // ── Filtros ───────────────────────────────────────────────────────────────
    [ObservableProperty] private string           _filtroTexto     = string.Empty;
    [ObservableProperty] private int              _filtroTipoIndex = 0;
    [ObservableProperty] private DateTimeOffset?  _filtroDataInicio = DateTimeOffset.Now.AddDays(-30);
    [ObservableProperty] private DateTimeOffset?  _filtroDataFim    = DateTimeOffset.Now;

    // ── Tabela ────────────────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<LancamentoCaixaItem> _lancamentosFiltrados = new();
    [ObservableProperty] private string _totalLancamentosTexto = string.Empty;

    // ── Estado ────────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isLoading       = false;
    [ObservableProperty] private bool   _temErro         = false;
    [ObservableProperty] private string _erroMensagem    = string.Empty;

    // ── Paginação ─────────────────────────────────────────────────────────────
    private int _totalRegistos = 0;
    private int _paginaAtual   = 1;
    private const int TamPagina = 50;

    // ─────────────────────────────────────────────────────────────────────────
    // CONSTRUTORES
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Construtor principal — DI em produção.</summary>
    public CaixaViewModel(LancamentoService lancamentoSvc)
    {
        _lancamentoSvc = lancamentoSvc;
        _ = CarregarAsync();
    }

    /// <summary>Construtor sem parâmetros — apenas para o Avalonia Designer.</summary>
    public CaixaViewModel()
    {
        var stubStore = new TokenStore();
        var stubApi   = new ApiClient(stubStore);
        _lancamentoSvc = new LancamentoService(stubApi);

        // Dados de demo para o designer
        LancamentosFiltrados = new ObservableCollection<LancamentoCaixaItem>
        {
            new(1, "01/06/2026", "Entrada", "Venda de produto",         "Venda de produto",    "Banco BIC", 500_000m, 500_000m, new DateTime(2026,6,1)),
            new(2, "02/06/2026", "Saída",   "Compra de matéria-prima",  "Despesas gerais",     "Banco BAI", 200_000m, 300_000m, new DateTime(2026,6,2)),
        };
        TotalLancamentosTexto = "2 lançamento(s)";
        SaldoCaixa   = "300.000 Kzs";
        EntradasDia  = "500.000 Kzs";
        SaidasDia    = "200.000 Kzs";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CARREGAMENTO
    // ─────────────────────────────────────────────────────────────────────────

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

            // Calcula saldo acumulado localmente (lista ordenada desc — invertemos)
            var ordenados = resp.Items
                .OrderBy(l => l.Data)
                .ToList();

            decimal saldoAcum = 0;
            var mapeados = ordenados.Select(l =>
            {
                saldoAcum += l.Tipo == "Entrada" ? l.Valor : -l.Valor;
                return new LancamentoCaixaItem(
                    Id:          l.Id,
                    Data:        l.Data.ToString("dd/MM/yyyy"),
                    Tipo:        l.Tipo,
                    Descricao:   l.Descricao,
                    Categoria:   l.Categoria,
                    Conta:       l.ContaBancariaNome ?? "—",
                    ValorBruto:  l.Valor,
                    SaldoAcum:   saldoAcum,
                    DataOrigem:  l.Data);
            })
            .OrderByDescending(l => l.DataOrigem)
            .ToList();

            LancamentosFiltrados = new ObservableCollection<LancamentoCaixaItem>(mapeados);
            TotalLancamentosTexto = $"{resp.Total} lançamento(s)";

            // Métricas do topo
            var hoje     = DateTime.Today;
            var entradas = resp.Items.Where(l => l.Tipo == "Entrada").Sum(l => l.Valor);
            var saidas   = resp.Items.Where(l => l.Tipo == "Saída").Sum(l => l.Valor);
            var saldo    = entradas - saidas;

            SaldoCaixa  = $"{saldo:N0} Kzs";
            EntradasDia = $"{entradas:N0} Kzs";
            SaidasDia   = $"{saidas:N0} Kzs";
        }
        catch (ApiException ex)
        {
            TemErro      = true;
            ErroMensagem = $"Erro da API ({(int)ex.StatusCode}): {ex.Message}";
        }
        catch (HttpRequestException)
        {
            TemErro      = true;
            ErroMensagem = $"Sem ligação à API ({ApiClient.BaseUrl}). Verifique o servidor.";
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

    // ─────────────────────────────────────────────────────────────────────────
    // COMANDOS
    // ─────────────────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task FiltrarAsync() => await CarregarAsync(1);

    [RelayCommand]
    private async Task RefrescarAsync() => await CarregarAsync(_paginaAtual);

    [RelayCommand]
    private void NovaEntrada() => OpenNovoLancamento?.Invoke();

    [RelayCommand]
    private void NovaSaida() => OpenNovoLancamento?.Invoke();

    // Chamado pelo CaixaView quando um novo lançamento é criado no modal
    public async Task OnLancamentoCriadoAsync() => await CarregarAsync(1);

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    private static ISeries[] Sparkline(double[] values, SKColor color) =>
        new ISeries[]
        {
            new LineSeries<double>
            {
                Values         = values,
                Stroke         = new SolidColorPaint(color) { StrokeThickness = 2 },
                Fill           = null,
                GeometrySize   = 0,
                LineSmoothness = 1
            }
        };
}

// ── Record de linha da tabela ─────────────────────────────────────────────────
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
    public bool   IsEntrada       => Tipo == "Entrada";
    public bool   IsSaida         => Tipo == "Saída";
    public string ValorFormatado  => IsEntrada ? $"+{ValorBruto:N0} Kzs" : $"-{ValorBruto:N0} Kzs";
    public string SaldoAcumulado  => $"{SaldoAcum:N0} Kzs";
    public string CorValor        => IsEntrada ? "#43A047" : "#E53935";
}