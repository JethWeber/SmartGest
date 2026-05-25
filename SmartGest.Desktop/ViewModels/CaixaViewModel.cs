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

public partial class CaixaViewModel : ViewModelBase
{
    // ── Eventos principais ────────────────────────────────────────────────────
    public event Action? OpenNovoLancamento;

    // ── Métricas do Topo ─────────────────────────────────────────────────────
    [ObservableProperty] private string _saldoCaixa    = "2.340.000 Kzs";
    [ObservableProperty] private string _entradasDia   = "480.000 Kzs";
    [ObservableProperty] private string _saidasDia     = "210.000 Kzs";
    [ObservableProperty] private string _entradasVariacao = "+12%";
    [ObservableProperty] private string _saidasVariacao   = "+5%";

    // ── Sparklines dos Cards ──────────────────────────────────────────────────
    public ISeries[] SparklineSaldo    { get; }
    public ISeries[] SparklineEntradas { get; }
    public ISeries[] SparklineSaidas   { get; }

    // ── Filtros ───────────────────────────────────────────────────────────────
    [ObservableProperty] private string  _filtroTexto     = string.Empty;
    [ObservableProperty] private int     _filtroTipoIndex = 0;         // 0=Todos,1=Entradas,2=Saídas
    [ObservableProperty] private DateTimeOffset? _filtroDataInicio = DateTimeOffset.Now.AddDays(-30);
    [ObservableProperty] private DateTimeOffset? _filtroDataFim    = DateTimeOffset.Now;

    // ── Tabela ────────────────────────────────────────────────────────────────
    /// <summary>Todos os lançamentos (fonte de verdade).</summary>
    private readonly ObservableCollection<LancamentoCaixaItem> _todos;

    /// <summary>Subconjunto exibido na DataGrid (após aplicar filtros).</summary>
    [ObservableProperty] private ObservableCollection<LancamentoCaixaItem> _lancamentosFiltrados;

    [ObservableProperty] private string _totalLancamentosTexto = string.Empty;

    // ─────────────────────────────────────────────────────────────────────────
    public CaixaViewModel()
    {
        // ── 1. SPARKLINES ─────────────────────────────────────────────────────
        SparklineSaldo    = new ISeries[] { Sparkline(new double[] { 5, 6, 5.5, 7, 8, 9, 8.5, 10 }, new SKColor(0x1A, 0x2E, 0x5A)) };
        SparklineEntradas = new ISeries[] { Sparkline(new double[] { 3, 4, 5,   4, 6,  7, 6,   8 }, SKColors.MediumSeaGreen) };
        SparklineSaidas   = new ISeries[] { Sparkline(new double[] { 2, 3, 2,   4, 3,  4, 3.5, 5 }, SKColors.Tomato) };

        // ── 2. DADOS DE DEMONSTRAÇÃO ───────────────────────────────────────────
        _todos = new ObservableCollection<LancamentoCaixaItem>(GerarDemoData());
        _lancamentosFiltrados = new ObservableCollection<LancamentoCaixaItem>(_todos);
        AtualizarContador();
    }

    // ── Comandos ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void NovaEntrada() => OpenNovoLancamento?.Invoke();

    [RelayCommand]
    private void NovaSaida() => OpenNovoLancamento?.Invoke();

    [RelayCommand]
    private void Filtrar()
    {
        var query = _todos.AsEnumerable();

        // Filtro de texto (descrição + conta)
        if (!string.IsNullOrWhiteSpace(FiltroTexto))
        {
            var termo = FiltroTexto.Trim().ToLower();
            query = query.Where(l =>
                l.Descricao.ToLower().Contains(termo) ||
                l.Conta.ToLower().Contains(termo)     ||
                l.Categoria.ToLower().Contains(termo));
        }

        // Filtro de tipo
        query = FiltroTipoIndex switch
        {
            1 => query.Where(l => l.IsEntrada),
            2 => query.Where(l => l.IsSaida),
            _ => query
        };

        // Filtro de datas
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

        LancamentosFiltrados = new ObservableCollection<LancamentoCaixaItem>(query);
        AtualizarContador();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void AtualizarContador()
    {
        TotalLancamentosTexto = $"{LancamentosFiltrados.Count} lançamento(s)";
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

    /// <summary>Gera 20 lançamentos de demo para MVP.</summary>
    private static LancamentoCaixaItem[] GerarDemoData()
    {
        var rng    = new Random(42);
        var contas = new[] { "ATL 021", "BPC 01", "BAI 03", "Caixa Física" };
        var cats   = new[] { "Venda de produto", "Compra de matéria-prima", "Despesas gerais",
                             "Pagamento de salários", "Recebimento de cliente", "Fornecedores" };

        double saldo = 0;
        var lista = new LancamentoCaixaItem[20];
        var hoje  = DateTime.Today;

        for (int i = 19; i >= 0; i--)
        {
            bool   isEntrada = rng.NextDouble() > 0.4;
            double valor     = rng.Next(50, 800) * 1000.0;
            saldo += isEntrada ? valor : -valor;

            lista[19 - i] = new LancamentoCaixaItem(
                Data:          hoje.AddDays(-i).ToString("dd/MM/yyyy"),
                Tipo:          isEntrada ? "Entrada" : "Saída",
                Descricao:     cats[rng.Next(cats.Length)],
                Categoria:     isEntrada ? "Receita" : "Despesa",
                Conta:         contas[rng.Next(contas.Length)],
                ValorBruto:    valor,
                SaldoAcum:     saldo,
                DataOrigem:    hoje.AddDays(-i)
            );
        }
        return lista;
    }
}

// ── Record de linha da tabela ─────────────────────────────────────────────────
public record LancamentoCaixaItem(
    string   Data,
    string   Tipo,
    string   Descricao,
    string   Categoria,
    string   Conta,
    double   ValorBruto,
    double   SaldoAcum,
    DateTime DataOrigem)
{
    public bool IsEntrada => Tipo == "Entrada";
    public bool IsSaida   => Tipo == "Saída";

    /// <summary>Valor com sinal, formatado em Kzs.</summary>
    public string ValorFormatado =>
        IsEntrada
            ? $"+{ValorBruto:N0} Kzs"
            : $"-{ValorBruto:N0} Kzs";

    /// <summary>Saldo acumulado formatado.</summary>
    public string SaldoAcumulado => $"{SaldoAcum:N0} Kzs";

    /// <summary>Cor do valor (verde = entrada, vermelho = saída).</summary>
    public string CorValor => IsEntrada ? "#43A047" : "#E53935";
}
