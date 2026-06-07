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

public partial class BalancoViewModel : ViewModelBase
{
    private readonly ContabilidadeService _svc;

    // ── Estado de carregamento ────────────────────────────────────────────────
    [ObservableProperty] private bool   _carregando   = false;
    [ObservableProperty] private string _erroMensagem = string.Empty;
    [ObservableProperty] private bool   _temErro      = false;

    // ── Período ───────────────────────────────────────────────────────────────
    [ObservableProperty] private int    _exercicio    = DateTime.Today.Year;
    [ObservableProperty] private int    _mesIndex     = DateTime.Today.Month - 1;
    [ObservableProperty] private string _periodoTexto = string.Empty;

    // ── Métricas do Topo ──────────────────────────────────────────────────────
    [ObservableProperty] private string _totalAtivoFmt          = "– Kzs";
    [ObservableProperty] private string _totalPassivoCapitalFmt = "– Kzs";
    [ObservableProperty] private string _resultadoExercicioFmt  = "– Kzs";
    [ObservableProperty] private string _corResultado           = "#43A047";
    [ObservableProperty] private bool   _estaEquilibrado        = true;
    [ObservableProperty] private string _textoEquilibrio        = "Balanceado ✓";
    [ObservableProperty] private string _corEquilibrio          = "#43A047";
    [ObservableProperty] private string _fundoEquilibrio        = "#E8F5E9";

    // ── Sparklines ────────────────────────────────────────────────────────────
    public ISeries[] SparklineAtivo          { get; } = Sparklines(new SKColor(0x21, 0x96, 0xF3));
    public ISeries[] SparklinePassivoCapital { get; } = Sparklines(new SKColor(0x1A, 0x2E, 0x5A));
    public ISeries[] SparklineResultado      { get; } = Sparklines(SKColors.MediumSeaGreen);

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

    [ObservableProperty] private double _totalCapitalProprio   = 0;
    [ObservableProperty] private double _totalPassivoMaisCapital = 0;

    public string TotalCapitalProprioFmt => $"{TotalCapitalProprio:N0} Kzs";

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor via DI
    public BalancoViewModel(ContabilidadeService svc)
    {
        _svc = svc;
    }

    // Constructor sem parâmetros para design-time / code-behind
    public BalancoViewModel() : this(App.Services.GetRequiredService<ContabilidadeService>()) { }

    // ── Inicialização assíncrona chamada pela View ────────────────────────────
    public async Task InicializarAsync()
        => await CarregarAsync();

    // ── Comandos ──────────────────────────────────────────────────────────────

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
            var resp = await _svc.ObterBalancoAsync(Exercicio, MesIndex + 1);

            if (resp is null)
            {
                MostrarErro("Sem resposta do servidor.");
                return;
            }

            // ── Preencher colecções ────────────────────────────────────────────
            Preencher(AtivoCorrentes,     resp.AtivoCorrentes);
            Preencher(AtivoNaoCorrentes,  resp.AtivoNaoCorrentes);
            Preencher(PassivosCorrentes,  resp.PassivosCorrentes);
            Preencher(PassivosNaoCorrentes, resp.PassivosNaoCorrentes);
            Preencher(CapitalProprio,     resp.CapitalProprio);

            // ── Totais ─────────────────────────────────────────────────────────
            TotalAtivoCorrente     = (double)resp.AtivoCorrentes.Sum(x => x.Valor);
            TotalAtivoNaoCorrente  = (double)resp.AtivoNaoCorrentes.Sum(x => x.Valor);
            TotalAtivo             = (double)resp.TotalAtivo;

            TotalPassivoCorrente    = (double)resp.PassivosCorrentes.Sum(x => x.Valor);
            TotalPassivoNaoCorrente = (double)resp.PassivosNaoCorrentes.Sum(x => x.Valor);
            TotalPassivo            = (double)resp.TotalPassivo;

            TotalCapitalProprio    = (double)resp.TotalCapital;
            TotalPassivoMaisCapital= (double)resp.TotalPassivoMaisCapital;

            // ── Métricas do topo ───────────────────────────────────────────────
            TotalAtivoFmt          = $"{resp.TotalAtivo:N0} Kzs";
            TotalPassivoCapitalFmt = $"{resp.TotalPassivoMaisCapital:N0} Kzs";

            var resultadoItem = resp.CapitalProprio
                .FirstOrDefault(x => x.Descricao.Contains("Resultado", StringComparison.OrdinalIgnoreCase));
            var resultado = resultadoItem?.Valor ?? 0m;
            ResultadoExercicioFmt = $"{resultado:N0} Kzs";
            CorResultado          = resultado >= 0 ? "#43A047" : "#E53935";

            bool equilibrado = Math.Abs(resp.TotalAtivo - resp.TotalPassivoMaisCapital) < 1m;
            EstaEquilibrado  = equilibrado;
            TextoEquilibrio  = equilibrado ? "Balanceado ✓" : "Desequilibrado ✗";
            CorEquilibrio    = equilibrado ? "#2E7D32" : "#C62828";
            FundoEquilibrio  = equilibrado ? "#E8F5E9"  : "#FFEBEE";

            // ── Período ────────────────────────────────────────────────────────
            var meses = new[] { "Jan","Fev","Mar","Abr","Mai","Jun","Jul","Ago","Set","Out","Nov","Dez" };
            PeriodoTexto = $"{meses[MesIndex]} {Exercicio}";

            // Notificar propriedades calculadas (sufixo Fmt)
            OnPropertyChanged(nameof(TotalAtivoCorrenteFmt));
            OnPropertyChanged(nameof(TotalAtivoNaoCorrenteFmt));
            OnPropertyChanged(nameof(TotalPassivoCorrenteFmt));
            OnPropertyChanged(nameof(TotalPassivoNaoCorrenteFmt));
            OnPropertyChanged(nameof(TotalCapitalProprioFmt));
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

    private static void Preencher(
        ObservableCollection<BalancoLinhaItem> coleccao,
        IEnumerable<BalancoLinhaResponse> fonte)
    {
        coleccao.Clear();
        foreach (var item in fonte)
            coleccao.Add(new BalancoLinhaItem(item.Descricao, (double)item.Valor, item.IsDeducao));
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

// ── Record de linha do balanço (inalterado — usado pela View) ─────────────────
public record BalancoLinhaItem(string Descricao, double Valor, bool IsDeducao = false)
{
    public string ValorFmt =>
        IsDeducao
            ? $"({Math.Abs(Valor):N0})"
            : $"{Valor:N0} Kzs";

    public string CorValor => IsDeducao ? "#E53935" : "#1A2E5A";
}
