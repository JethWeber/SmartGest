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

public partial class ContaseBancosViewModel : ViewModelBase
{
    // ── Métricas do Topo ─────────────────────────────────────────────────────
    [ObservableProperty] private string _saldoConsolidado = "12.480.000 Kzs";
    [ObservableProperty] private string _totalContas      = "4";
    [ObservableProperty] private string _movimentosMes   = "87";
    [ObservableProperty] private string _saldoVariacao   = "+8%";

    // ── Sparklines ───────────────────────────────────────────────────────────
    public ISeries[] SparklineSaldo      { get; }
    public ISeries[] SparklineMovimentos { get; }

    // ── Conta seleccionada (detalhe lateral) ──────────────────────────────
    [ObservableProperty] private ContaBancariaItem? _contaSelecionada;

    // ── Gráfico de evolução ───────────────────────────────────────────────
    [ObservableProperty] private ISeries[] _seriesEvolucao = Array.Empty<ISeries>();
    public Axis[] EixoX { get; }
    public Axis[] EixoY { get; }

    // ── Filtros de movimentos ─────────────────────────────────────────────
    [ObservableProperty] private string          _filtroTexto     = string.Empty;
    [ObservableProperty] private int             _filtroTipoIndex = 0;
    [ObservableProperty] private DateTimeOffset? _filtroDataInicio = DateTimeOffset.Now.AddDays(-30);
    [ObservableProperty] private DateTimeOffset? _filtroDataFim    = DateTimeOffset.Now;

    // ── Listas principais ─────────────────────────────────────────────────
    private readonly ObservableCollection<ContaBancariaItem> _todasContas;
    [ObservableProperty] private ObservableCollection<ContaBancariaItem> _contasFiltradas;

    private readonly ObservableCollection<MovimentoBancarioItem> _todosMovimentos;
    [ObservableProperty] private ObservableCollection<MovimentoBancarioItem> _movimentosFiltrados;
    [ObservableProperty] private string _totalMovimentosTexto = string.Empty;

    // ── Estado do Modal "Gerir Contas" ────────────────────────────────────
    [ObservableProperty] private bool   _modalGerirAberto = false;

    /// <summary>Cópia editável das contas, usada dentro do modal.</summary>
    [ObservableProperty] private ObservableCollection<ContaBancariaEditavel> _contasEditaveis = new();

    /// <summary>Conta a ser editada no formulário lateral do modal (null = modo Adicionar).</summary>
    [ObservableProperty] private ContaBancariaEditavel? _contaEmEdicao;

    // ── Campos do formulário do modal ─────────────────────────────────────
    [ObservableProperty] private string _formBanco    = string.Empty;
    [ObservableProperty] private string _formNIB      = string.Empty;
    [ObservableProperty] private string _formTipo     = "Conta à Ordem";
    [ObservableProperty] private string _formMoeda    = "AOA";
    [ObservableProperty] private string _formAgencia  = string.Empty;
    [ObservableProperty] private string _formTitular  = string.Empty;
    [ObservableProperty] private string _formCorAccent = "#1A2E5A";
    [ObservableProperty] private double _formSaldo    = 0;

    /// <summary>Título do painel de formulário dentro do modal.</summary>
    [ObservableProperty] private string _formTitulo       = "Nova Conta";
    [ObservableProperty] private bool   _formEdicaoActiva = false;

    /// <summary>Texto do botão de guardar — muda consoante o modo.</summary>
    public string FormBotaoTexto => FormEdicaoActiva ? "Actualizar" : "Adicionar";

    partial void OnFormEdicaoActivaChanged(bool value) => OnPropertyChanged(nameof(FormBotaoTexto));

    // ── Erros de validação simples ────────────────────────────────────────
    [ObservableProperty] private string _formErroBanco  = string.Empty;
    [ObservableProperty] private string _formErroNIB    = string.Empty;

    // ─────────────────────────────────────────────────────────────────────
    public ContaseBancosViewModel()
    {
        SparklineSaldo      = new ISeries[] { Sparkline(new double[] { 8, 9, 8.5, 10, 11, 10.5, 12, 12.5 }, new SKColor(0x1A, 0x2E, 0x5A)) };
        SparklineMovimentos = new ISeries[] { Sparkline(new double[] { 5, 7,  6,   8,  7,   9,   8,  10  }, new SKColor(0x21, 0x96, 0xF3)) };

        var meses = new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun",
                            "Jul", "Ago", "Set", "Out", "Nov", "Dez" };
        EixoX = new[] { new Axis { Labels = meses, TextSize = 11, LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB)) } };
        EixoY = new[] { new Axis { Labeler = v => $"{v:N0}k", TextSize = 11, LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB)) } };

        _todasContas        = new ObservableCollection<ContaBancariaItem>(GerarContas());
        _contasFiltradas    = new ObservableCollection<ContaBancariaItem>(_todasContas);
        _todosMovimentos    = new ObservableCollection<MovimentoBancarioItem>(GerarMovimentos());
        _movimentosFiltrados = new ObservableCollection<MovimentoBancarioItem>(_todosMovimentos);

        ContaSelecionada = _todasContas.FirstOrDefault();
        AtualizarGraficoEvolucao();
        AtualizarContador();
    }

    // ══════════════════════════════════════════════════════════════════════
    // COMANDOS — Lista / Selecção
    // ══════════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void SelecionarConta(ContaBancariaItem conta)
    {
        ContaSelecionada = conta;
        AtualizarGraficoEvolucao();
        MovimentosFiltrados = new ObservableCollection<MovimentoBancarioItem>(
            _todosMovimentos.Where(m => m.Banco == conta.Banco));
        AtualizarContador();
    }

    [RelayCommand]
    private void Filtrar()
    {
        var query = _todosMovimentos.AsEnumerable();

        if (ContaSelecionada is not null)
            query = query.Where(m => m.Banco == ContaSelecionada.Banco);

        if (!string.IsNullOrWhiteSpace(FiltroTexto))
        {
            var termo = FiltroTexto.Trim().ToLower();
            query = query.Where(m =>
                m.Descricao.ToLower().Contains(termo) ||
                m.Referencia.ToLower().Contains(termo));
        }

        query = FiltroTipoIndex switch
        {
            1 => query.Where(m => m.IsCredito),
            2 => query.Where(m => m.IsDebito),
            _ => query
        };

        if (FiltroDataInicio.HasValue)
            query = query.Where(m => m.DataOrigem >= FiltroDataInicio.Value.Date);
        if (FiltroDataFim.HasValue)
            query = query.Where(m => m.DataOrigem <= FiltroDataFim.Value.Date);

        MovimentosFiltrados = new ObservableCollection<MovimentoBancarioItem>(query);
        AtualizarContador();
    }

    [RelayCommand]
    private void Exportar() { /* ponto de extensão */ }

    // ══════════════════════════════════════════════════════════════════════
    // COMANDOS — Modal Gerir Contas
    // ══════════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void AbrirGerirContas()
    {
        // Cria uma cópia editável de cada conta
        ContasEditaveis = new ObservableCollection<ContaBancariaEditavel>(
            _todasContas.Select(c => new ContaBancariaEditavel(c)));

        // Abre o formulário em modo "Nova Conta"
        PrepararFormularioNovo();
        ModalGerirAberto = true;
    }

    [RelayCommand]
    private void FecharModal() => ModalGerirAberto = false;

    [RelayCommand]
    private void LimparFormulario() => PrepararFormularioNovo();

    /// <summary>Clique em "Editar" numa linha da lista do modal.</summary>
    [RelayCommand]
    private void EditarContaModal(ContaBancariaEditavel conta)
    {
        ContaEmEdicao  = conta;
        FormTitulo     = "Editar Conta";
        FormEdicaoActiva = true;

        FormBanco    = conta.Banco;
        FormNIB      = conta.NIB;
        FormTipo     = conta.Tipo;
        FormMoeda    = conta.Moeda;
        FormAgencia  = conta.Agencia;
        FormTitular  = conta.Titular;
        FormCorAccent = conta.CorAccent;
        FormSaldo    = conta.SaldoAtual;

        LimparErros();
    }

    /// <summary>Clique em "Eliminar" numa linha da lista do modal.</summary>
    [RelayCommand]
    private void EliminarContaModal(ContaBancariaEditavel conta)
    {
        ContasEditaveis.Remove(conta);
        if (ContaEmEdicao == conta)
            PrepararFormularioNovo();
    }

    /// <summary>Guarda (cria ou actualiza) a conta no formulário do modal.</summary>
    [RelayCommand]
    private void GuardarConta()
    {
        if (!ValidarFormulario()) return;

        if (ContaEmEdicao is not null)
        {
            // Actualiza a conta em edição
            ContaEmEdicao.Banco     = FormBanco;
            ContaEmEdicao.NIB       = FormNIB;
            ContaEmEdicao.Tipo      = FormTipo;
            ContaEmEdicao.Moeda     = FormMoeda;
            ContaEmEdicao.Agencia   = FormAgencia;
            ContaEmEdicao.Titular   = FormTitular;
            ContaEmEdicao.CorAccent = FormCorAccent;
            ContaEmEdicao.SaldoAtual = FormSaldo;
            ContaEmEdicao.RefreshIniciais();
        }
        else
        {
            // Nova conta
            ContasEditaveis.Add(new ContaBancariaEditavel(
                banco:     FormBanco,
                NIB:       FormNIB,
                tipo:      FormTipo,
                moeda:     FormMoeda,
                agencia:   FormAgencia,
                titular:   FormTitular,
                corAccent: FormCorAccent,
                saldoAtual: FormSaldo
            ));
        }

        PrepararFormularioNovo();
    }

    /// <summary>Aplica todas as alterações do modal à lista principal e fecha.</summary>
    [RelayCommand]
    private void AplicarAlteracoes()
    {
        _todasContas.Clear();
        foreach (var e in ContasEditaveis)
            _todasContas.Add(e.ToItem());

        ContasFiltradas  = new ObservableCollection<ContaBancariaItem>(_todasContas);
        TotalContas      = _todasContas.Count.ToString();
        ContaSelecionada = _todasContas.FirstOrDefault();
        AtualizarGraficoEvolucao();
        ModalGerirAberto = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════

    private void PrepararFormularioNovo()
    {
        ContaEmEdicao    = null;
        FormTitulo       = "Nova Conta";
        FormEdicaoActiva = false;
        FormBanco        = string.Empty;
        FormNIB          = string.Empty;
        FormTipo         = "Conta à Ordem";
        FormMoeda        = "AOA";
        FormAgencia      = string.Empty;
        FormTitular      = string.Empty;
        FormCorAccent    = "#1A2E5A";
        FormSaldo        = 0;
        LimparErros();
    }

    private bool ValidarFormulario()
    {
        LimparErros();
        bool ok = true;

        if (string.IsNullOrWhiteSpace(FormBanco))
        {
            FormErroBanco = "O nome do banco é obrigatório.";
            ok = false;
        }
        if (string.IsNullOrWhiteSpace(FormNIB))
        {
            FormErroNIB = "O NIB é obrigatório.";
            ok = false;
        }
        return ok;
    }

    private void LimparErros()
    {
        FormErroBanco = string.Empty;
        FormErroNIB   = string.Empty;
    }

    private void AtualizarContador()
        => TotalMovimentosTexto = $"{MovimentosFiltrados.Count} movimento(s)";

    private void AtualizarGraficoEvolucao()
    {
        if (ContaSelecionada is null) return;

        var rng    = new Random(ContaSelecionada.Banco.GetHashCode());
        var base_  = ContaSelecionada.SaldoAtual / 1_000_000;
        var values = Enumerable.Range(0, 12)
                               .Select(_ => Math.Round(base_ * (0.7 + rng.NextDouble() * 0.6), 2))
                               .ToArray();

        SeriesEvolucao = new ISeries[]
        {
            new LineSeries<double>
            {
                Name           = "Saldo",
                Values         = values,
                Stroke         = new SolidColorPaint(new SKColor(0x1A, 0x2E, 0x5A)) { StrokeThickness = 2 },
                Fill           = new SolidColorPaint(new SKColor(0x1A, 0x2E, 0x5A, 0x18)),
                GeometrySize   = 6,
                GeometryFill   = new SolidColorPaint(SKColors.White),
                GeometryStroke = new SolidColorPaint(new SKColor(0x1A, 0x2E, 0x5A)) { StrokeThickness = 2 },
                LineSmoothness = 0.5
            }
        };
    }

    private static LineSeries<double> Sparkline(double[] values, SKColor color) =>
        new LineSeries<double>
        {
            Values = values,
            Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
            Fill   = null, GeometrySize = 0, LineSmoothness = 1
        };

    // ── Dados de demo ─────────────────────────────────────────────────────

    private static ContaBancariaItem[] GerarContas() => new[]
    {
        new ContaBancariaItem("Banco BIC", "AO06.0055.0000.1234.5678.9012.3", "Conta à Ordem",    "AOA", 4_820_000, 4_600_000, "Luanda — Maianga",    "SmartGest, Lda.", "Bank",        "#1A2E5A"),
        new ContaBancariaItem("Banco BAI", "AO06.0040.0000.9876.5432.1098.7", "Conta à Ordem",    "AOA", 3_150_000, 3_200_000, "Luanda — Ingombota",  "SmartGest, Lda.", "Bank",        "#0D47A1"),
        new ContaBancariaItem("Banco BPC", "AO06.0038.0000.1111.2222.3333.4", "Depósito a Prazo", "AOA", 2_980_000, 2_980_000, "Luanda — Samba",      "SmartGest, Lda.", "BankOutline", "#1B5E20"),
        new ContaBancariaItem("Banco ATL", "AO06.0006.0000.4444.5555.6666.7", "Conta à Ordem",    "USD", 1_530_000, 1_490_000, "Luanda — Talatona",   "SmartGest, Lda.", "Bank",        "#E65100"),
    };

    private static MovimentoBancarioItem[] GerarMovimentos()
    {
        var rng    = new Random(13);
        var bancos = new[] { "Banco BIC", "Banco BAI", "Banco BPC", "Banco ATL" };
        var descs  = new[] { "Pagamento a fornecedor", "Recebimento de cliente", "Transferência interna",
                             "Pagamento de salários",  "Recebimento de serviços", "Imposto — IRT",
                             "Compra de material",     "Juros creditados" };
        var refs   = new[] { "TRF", "CHQ", "DEP", "PAG", "REC" };
        var hoje   = DateTime.Today;
        var lista  = new MovimentoBancarioItem[40];

        for (int i = 0; i < 40; i++)
        {
            bool isCred = rng.NextDouble() > 0.45;
            double valor = rng.Next(20, 600) * 1000.0;
            lista[i] = new MovimentoBancarioItem(
                Data:       hoje.AddDays(-rng.Next(0, 30)).ToString("dd/MM/yyyy"),
                Banco:      bancos[rng.Next(bancos.Length)],
                Descricao:  descs[rng.Next(descs.Length)],
                Referencia: $"{refs[rng.Next(refs.Length)]}-{rng.Next(10000, 99999)}",
                Tipo:       isCred ? "Crédito" : "Débito",
                Valor:      valor,
                DataOrigem: hoje.AddDays(-rng.Next(0, 30)));
        }
        return lista;
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// ContaBancariaEditavel — versão mutável para o formulário do modal
// ══════════════════════════════════════════════════════════════════════════════

public partial class ContaBancariaEditavel : ObservableObject
{
    [ObservableProperty] private string _banco;
    [ObservableProperty] private string _nIB;
    [ObservableProperty] private string _tipo;
    [ObservableProperty] private string _moeda;
    [ObservableProperty] private string _agencia;
    [ObservableProperty] private string _titular;
    [ObservableProperty] private string _corAccent;
    [ObservableProperty] private double _saldoAtual;
    [ObservableProperty] private string _iniciais;

    public ContaBancariaEditavel(ContaBancariaItem item)
    {
        _banco     = item.Banco;
        _nIB       = item.NIB;
        _tipo      = item.Tipo;
        _moeda     = item.Moeda;
        _agencia   = item.Agencia;
        _titular   = item.Titular;
        _corAccent = item.CorAccent;
        _saldoAtual = item.SaldoAtual;
        _iniciais  = item.Iniciais;
    }

    public ContaBancariaEditavel(
        string banco, string NIB, string tipo, string moeda,
        string agencia, string titular, string corAccent, double saldoAtual)
    {
        _banco     = banco;
        _nIB       = NIB;
        _tipo      = tipo;
        _moeda     = moeda;
        _agencia   = agencia;
        _titular   = titular;
        _corAccent = corAccent;
        _saldoAtual = saldoAtual;
        _iniciais  = CalcIniciais(banco);
    }

    public void RefreshIniciais() => Iniciais = CalcIniciais(Banco);

    private static string CalcIniciais(string banco) =>
        string.Concat(banco.Split(' ').Where(w => w.Length > 0).Take(2).Select(w => w[0]));

    public string SaldoFmt => $"{SaldoAtual:N0} {Moeda}";

    public ContaBancariaItem ToItem() =>
        new ContaBancariaItem(Banco, NIB, Tipo, Moeda, SaldoAtual, SaldoAtual,
                              Agencia, Titular, "Bank", CorAccent);
}

// ══════════════════════════════════════════════════════════════════════════════
// Records imutáveis
// ══════════════════════════════════════════════════════════════════════════════

public record ContaBancariaItem(
    string Banco, string NIB, string Tipo, string Moeda,
    double SaldoAtual, double SaldoOntem,
    string Agencia, string Titular, string Icone, string CorAccent)
{
    public string SaldoFmt      => $"{SaldoAtual:N0} {Moeda}";
    public double Variacao      => SaldoAtual - SaldoOntem;
    public string VariacaoFmt   => Variacao >= 0 ? $"+{Variacao:N0} {Moeda}" : $"-{Math.Abs(Variacao):N0} {Moeda}";
    public string CorVariacao   => Variacao >= 0 ? "#43A047" : "#E53935";
    public string FundoVariacao => Variacao >= 0 ? "#E8F5E9"  : "#FFEBEE";
    public string Iniciais      => string.Concat(Banco.Split(' ').Where(w => w.Length > 0).Take(2).Select(w => w[0]));
}

public record MovimentoBancarioItem(
    string Data, string Banco, string Descricao,
    string Referencia, string Tipo, double Valor, DateTime DataOrigem)
{
    public bool   IsCredito  => Tipo == "Crédito";
    public bool   IsDebito   => Tipo == "Débito";
    public string ValorFmt   => IsCredito ? $"+{Valor:N0} Kzs" : $"-{Valor:N0} Kzs";
    public string CorValor   => IsCredito ? "#43A047" : "#E53935";
    public string FundoPill  => IsCredito ? "#E8F5E9"  : "#FFEBEE";
    public string CorPill    => IsCredito ? "#43A047"  : "#E53935";
}