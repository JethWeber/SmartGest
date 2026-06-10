using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartGest.Desktop.Services;
using static SmartGest.Desktop.Services.ContasBancariasService;

namespace SmartGest.Desktop.ViewModels;

public partial class ContaseBancosViewModel : ViewModelBase
{
    private readonly ContasBancariasService _service;

    // ── Métricas do Topo ─────────────────────────────────────────────────────
    [ObservableProperty] private string _saldoConsolidado = "— Kzs";
    [ObservableProperty] private string _totalContas      = "—";
    [ObservableProperty] private string _movimentosMes   = "—";
    [ObservableProperty] private string _saldoVariacao   = "";

    // ── Estado de carregamento / erro ────────────────────────────────────────
    [ObservableProperty] private bool   _isLoading    = false;
    [ObservableProperty] private string _erroMensagem = string.Empty;

    // ── Sparklines ───────────────────────────────────────────────────────────
    public ISeries[] SparklineSaldo      { get; }
    public ISeries[] SparklineMovimentos { get; }

    // ── Conta seleccionada (detalhe lateral) ─────────────────────────────────
    [ObservableProperty] private ContaBancariaItem? _contaSelecionada;

    // Notifica a View quando a conta seleccionada muda (para actualizar os itens activos)
    partial void OnContaSelecionadaChanged(ContaBancariaItem? value)
    {
        // Força re-avaliação da propriedade IsActiva em todos os itens da lista
        foreach (var c in ContasFiltradas)
            c.NotifyIsActiva(value);
    }

    // ── Gráfico de evolução ──────────────────────────────────────────────────
    [ObservableProperty] private ISeries[] _seriesEvolucao = Array.Empty<ISeries>();
    public Axis[] EixoX { get; }
    public Axis[] EixoY { get; }

    // ── Filtros de movimentos ────────────────────────────────────────────────
    [ObservableProperty] private string          _filtroTexto      = string.Empty;
    [ObservableProperty] private int             _filtroTipoIndex  = 0;
    [ObservableProperty] private DateTimeOffset? _filtroDataInicio = DateTimeOffset.Now.AddDays(-30);
    [ObservableProperty] private DateTimeOffset? _filtroDataFim    = DateTimeOffset.Now;

    // ── Listas principais ────────────────────────────────────────────────────
    private readonly ObservableCollection<ContaBancariaItem> _todasContas = new();
    [ObservableProperty] private ObservableCollection<ContaBancariaItem> _contasFiltradas = new();

    private readonly ObservableCollection<MovimentoBancarioItem> _todosMovimentos = new();
    [ObservableProperty] private ObservableCollection<MovimentoBancarioItem> _movimentosFiltrados = new();
    [ObservableProperty] private string _totalMovimentosTexto = string.Empty;

    // ── Estado do Modal "Gerir Contas" ───────────────────────────────────────
    [ObservableProperty] private bool _modalGerirAberto = false;

    [ObservableProperty] private ObservableCollection<ContaBancariaEditavel> _contasEditaveis = new();
    [ObservableProperty] private ContaBancariaEditavel? _contaEmEdicao;

    // ── Campos do formulário do modal ────────────────────────────────────────
    [ObservableProperty] private string _formBanco     = string.Empty;
    [ObservableProperty] private string _formNIB       = string.Empty;
    [ObservableProperty] private string _formTipo      = "Conta à Ordem";
    [ObservableProperty] private string _formMoeda     = "AOA";
    [ObservableProperty] private string _formAgencia   = string.Empty;
    [ObservableProperty] private string _formTitular   = string.Empty;
    [ObservableProperty] private string _formCorAccent = "#1A2E5A";
    [ObservableProperty] private string _formSaldoTexto = "0";
    [ObservableProperty] private string _formTitulo       = "Nova Conta";
    [ObservableProperty] private bool   _formEdicaoActiva = false;

    public string FormBotaoTexto => FormEdicaoActiva ? "Actualizar" : "Adicionar";
    partial void OnFormEdicaoActivaChanged(bool value) => OnPropertyChanged(nameof(FormBotaoTexto));

    // ── Erros de validação do formulário ─────────────────────────────────────
    [ObservableProperty] private string _formErroBanco   = string.Empty;
    [ObservableProperty] private string _formErroNIB     = string.Empty;
    [ObservableProperty] private string _formErroTitular = string.Empty;
    [ObservableProperty] private string _formErroSaldo   = string.Empty;

    // ── Estado de gravação do modal ──────────────────────────────────────────
    [ObservableProperty] private bool   _isSaving      = false;
    [ObservableProperty] private string _erroModal     = string.Empty;
    [ObservableProperty] private string _sucessoModal  = string.Empty;

    // ── Listas para ComboBox (instância — {Binding} não resolve propriedades estáticas)
    public IReadOnlyList<string> TiposConta { get; } = new[]
    {
        "Conta à Ordem", "Depósito a Prazo", "Conta Poupança", "Conta Empresarial"
    };

    public IReadOnlyList<string> MoedasDisponiveis { get; } = new[]
    {
        "AOA", "USD", "EUR"
    };

    // ─────────────────────────────────────────────────────────────────────────
    public ContaseBancosViewModel(ContasBancariasService service)
    {
        _service = service;

        SparklineSaldo      = new ISeries[] { Sparkline(new double[] { 8, 9, 8.5, 10, 11, 10.5, 12, 12.5 }, new SKColor(0x1A, 0x2E, 0x5A)) };
        SparklineMovimentos = new ISeries[] { Sparkline(new double[] { 5, 7,  6,   8,  7,   9,   8,  10  }, new SKColor(0x21, 0x96, 0xF3)) };

        var meses = new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun",
                            "Jul", "Ago", "Set", "Out", "Nov", "Dez" };
        EixoX = new[] { new Axis { Labels = meses, TextSize = 11, LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB)) } };
        EixoY = new[] { new Axis { Labeler = v => $"{v:N0}k", TextSize = 11, LabelsPaint = new SolidColorPaint(new SKColor(0x9A, 0xA0, 0xAB)) } };

        _ = CarregarContasAsync();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // CARREGAMENTO INICIAL
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Alias público chamado pelo MainWindowViewModel quando navega para esta
    /// página (OnSelectedMenuIndexChanged). Delega em CarregarContasAsync.
    /// </summary>
    public Task ActivarAsync() => CarregarContasAsync();

    [RelayCommand]
    private async Task CarregarContasAsync()
    {
        IsLoading    = true;
        ErroMensagem = string.Empty;

        try
        {
            var sumario = await _service.ListarAsync();
            if (sumario is null)
            {
                ErroMensagem = "Não foi possível obter as contas bancárias.";
                return;
            }

            _todasContas.Clear();
            foreach (var dto in sumario.Contas)
                _todasContas.Add(MapDto(dto));

            ContasFiltradas  = new ObservableCollection<ContaBancariaItem>(_todasContas);
            SaldoConsolidado = $"{sumario.SaldoConsolidado:N0} Kzs";
            TotalContas      = sumario.TotalContas.ToString();
            MovimentosMes    = sumario.MovimentosMes.ToString();

            // Seleccionar a primeira conta e notificar IsActiva
            var primeira = _todasContas.FirstOrDefault();
            if (primeira is not null)
            {
                ContaSelecionada = primeira;
                foreach (var c in ContasFiltradas)
                    c.NotifyIsActiva(ContaSelecionada);
                await CarregarMovimentosAsync(primeira.Id);
            }

            AtualizarGraficoEvolucao();
        }
        catch (Exception ex)
        {
            ErroMensagem = $"Erro ao carregar contas: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CarregarMovimentosAsync(int contaId)
    {
        _todosMovimentos.Clear();

        string?   tipo     = FiltroTipoIndex == 1 ? "Crédito" : FiltroTipoIndex == 2 ? "Débito" : null;
        DateTime? dataIni  = FiltroDataInicio?.Date;
        DateTime? dataFim  = FiltroDataFim?.Date;
        string?   texto    = string.IsNullOrWhiteSpace(FiltroTexto) ? null : FiltroTexto.Trim();

        try
        {
            var movimentos = await _service.ListarMovimentosAsync(contaId, tipo, dataIni, dataFim, texto);
            if (movimentos is null) return;

            foreach (var m in movimentos)
                _todosMovimentos.Add(MapMovimento(m));

            MovimentosFiltrados = new ObservableCollection<MovimentoBancarioItem>(_todosMovimentos);
            AtualizarContador();
        }
        catch (Exception ex)
        {
            ErroMensagem = $"Erro ao carregar movimentos: {ex.Message}";
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // COMANDOS — Lista / Selecção
    // ══════════════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task SelecionarConta(ContaBancariaItem conta)
    {
        ContaSelecionada = conta;
        // Notifica todos os itens para que actualizem o estado activo na View
        foreach (var c in ContasFiltradas)
            c.NotifyIsActiva(ContaSelecionada);

        AtualizarGraficoEvolucao();
        await CarregarMovimentosAsync(conta.Id);
    }

    [RelayCommand]
    private async Task Filtrar()
    {
        if (ContaSelecionada is null) return;
        await CarregarMovimentosAsync(ContaSelecionada.Id);
    }

    [RelayCommand]
    private void Exportar() { /* ponto de extensão */ }

    // ══════════════════════════════════════════════════════════════════════════
    // COMANDOS — Modal Gerir Contas
    // ══════════════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void AbrirGerirContas()
    {
        ContasEditaveis = new ObservableCollection<ContaBancariaEditavel>(
            _todasContas.Select(c => new ContaBancariaEditavel(c)));

        PrepararFormularioNovo();
        LimparFeedbackModal();
        ModalGerirAberto = true;
    }

    [RelayCommand]
    private void FecharModal() => ModalGerirAberto = false;

    [RelayCommand]
    private void LimparFormulario() => PrepararFormularioNovo();

    [RelayCommand]
    private void EditarContaModal(ContaBancariaEditavel conta)
    {
        ContaEmEdicao    = conta;
        FormTitulo       = "Editar Conta";
        FormEdicaoActiva = true;

        FormBanco      = conta.Banco;
        FormNIB        = conta.NIB;
        FormTipo       = conta.Tipo;
        FormMoeda      = conta.Moeda;
        FormAgencia    = conta.Agencia;
        FormTitular    = conta.Titular;
        FormCorAccent  = conta.CorAccent;
        FormSaldoTexto = conta.SaldoAtual.ToString("F2");

        LimparErros();
        LimparFeedbackModal();
    }

    [RelayCommand]
    private async Task EliminarContaModal(ContaBancariaEditavel conta)
    {
        LimparFeedbackModal();
        try
        {
            if (conta.Id > 0)
                await _service.EliminarAsync(conta.Id);

            ContasEditaveis.Remove(conta);
            if (ContaEmEdicao == conta)
                PrepararFormularioNovo();

            MostrarSucesso($"Conta «{conta.Banco}» eliminada com sucesso.");

            try { await SincronizarListaPrincipalAsync(); }
            catch { /* eliminada na API; lista actualiza ao navegar */ }
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            ErroModal = "Sem permissão para eliminar contas ou sessão expirada.";
        }
        catch (ApiException ex)
        {
            ErroModal = $"Erro ao eliminar: {ex.Message}";
        }
        catch (HttpRequestException)
        {
            ErroModal = "Sem ligação à API. Verifique se o servidor está activo.";
        }
        catch (Exception ex)
        {
            ErroModal = $"Erro ao eliminar: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task GuardarConta()
    {
        LimparFeedbackModal();

        if (!ValidarFormulario()) return;

        IsSaving = true;

        // Parse seguro do saldo — validação já garantiu que é número válido
        decimal saldo = decimal.TryParse(
            FormSaldoTexto.Replace(',', '.'),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed) ? parsed : 0;

        var emEdicao = ContaEmEdicao is not null && ContaEmEdicao.Id > 0;

        try
        {
            var req = new ContaBancariaRequest(
                Banco:      FormBanco.Trim(),
                NIB:        FormNIB.Trim(),
                Tipo:       FormTipo,
                Moeda:      FormMoeda,
                SaldoAtual: saldo,
                Agencia:    FormAgencia.Trim(),
                Titular:    FormTitular.Trim(),
                CorAccent:  FormCorAccent.Trim());

            if (emEdicao)
            {
                var updated = await _service.AtualizarAsync(ContaEmEdicao!.Id, req)
                    ?? throw new ApiException(HttpStatusCode.BadGateway,
                        "A API não devolveu dados da conta actualizada.");

                ContaEmEdicao.Banco      = updated.Banco;
                ContaEmEdicao.NIB        = updated.NIB;
                ContaEmEdicao.Tipo       = updated.Tipo;
                ContaEmEdicao.Moeda      = updated.Moeda;
                ContaEmEdicao.Agencia    = updated.Agencia;
                ContaEmEdicao.Titular    = updated.Titular;
                ContaEmEdicao.CorAccent  = updated.CorAccent;
                ContaEmEdicao.SaldoAtual = (double)updated.SaldoAtual;
                ContaEmEdicao.RefreshIniciais();

                MostrarSucesso($"Conta «{updated.Banco}» actualizada com sucesso.");
            }
            else
            {
                var created = await _service.CriarAsync(req)
                    ?? throw new ApiException(HttpStatusCode.BadGateway,
                        "A API não devolveu dados da nova conta.");

                ContasEditaveis.Add(new ContaBancariaEditavel(
                    id:        created.Id,
                    banco:     created.Banco,
                    NIB:       created.NIB,
                    tipo:      created.Tipo,
                    moeda:     created.Moeda,
                    agencia:   created.Agencia,
                    titular:   created.Titular,
                    corAccent: created.CorAccent,
                    saldoAtual:(double)created.SaldoAtual));

                MostrarSucesso($"Conta «{created.Banco}» adicionada com sucesso.");
            }

            PrepararFormularioNovo();

            try { await SincronizarListaPrincipalAsync(); }
            catch { /* guardado na API; lista actualiza ao navegar */ }
        }
        catch (ApiException ex) when (ex.IsConflict)
        {
            ErroModal = "Já existe uma conta com este NIB.";
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            ErroModal = "Sem permissão para gerir contas ou sessão expirada. Inicie sessão novamente.";
        }
        catch (ApiException ex)
        {
            ErroModal = ex.Message.Contains("NIB", StringComparison.OrdinalIgnoreCase)
                ? "Já existe uma conta com este NIB."
                : $"Erro ao guardar: {ex.Message}";
        }
        catch (HttpRequestException)
        {
            ErroModal = "Sem ligação à API. Verifique se o servidor está activo.";
        }
        catch (Exception ex)
        {
            ErroModal = $"Erro ao guardar: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>Fecha o modal e recarrega tudo da API para reflectir alterações.</summary>
    [RelayCommand]
    private async Task AplicarAlteracoes()
    {
        ModalGerirAberto = false;
        await CarregarContasAsync();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════════

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
        FormSaldoTexto   = "0";
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

        if (string.IsNullOrWhiteSpace(FormTitular))
        {
            FormErroTitular = "O titular é obrigatório.";
            ok = false;
        }

        if (!decimal.TryParse(
                FormSaldoTexto?.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var saldoParsed) || saldoParsed < 0)
        {
            FormErroSaldo = "Introduza um valor numérico válido (≥ 0).";
            ok = false;
        }

        if (!ok)
            ErroModal = "Preencha correctamente os campos obrigatórios assinalados com *.";

        return ok;
    }

    private void LimparErros()
    {
        FormErroBanco   = string.Empty;
        FormErroNIB     = string.Empty;
        FormErroTitular = string.Empty;
        FormErroSaldo   = string.Empty;
    }

    private void LimparFeedbackModal()
    {
        ErroModal    = string.Empty;
        SucessoModal = string.Empty;
    }

    private void MostrarSucesso(string mensagem)
    {
        ErroModal    = string.Empty;
        SucessoModal = mensagem;
    }

    /// <summary>Actualiza a sidebar e métricas sem fechar o modal.</summary>
    private async Task SincronizarListaPrincipalAsync()
    {
        var sumario = await _service.ListarAsync();
        if (sumario is null) return;

        _todasContas.Clear();
        foreach (var dto in sumario.Contas)
            _todasContas.Add(MapDto(dto));

        ContasFiltradas  = new ObservableCollection<ContaBancariaItem>(_todasContas);
        SaldoConsolidado = $"{sumario.SaldoConsolidado:N0} Kzs";
        TotalContas      = sumario.TotalContas.ToString();
        MovimentosMes    = sumario.MovimentosMes.ToString();

        var idSeleccionado = ContaSelecionada?.Id;
        ContaSelecionada = idSeleccionado.HasValue
            ? _todasContas.FirstOrDefault(c => c.Id == idSeleccionado.Value) ?? _todasContas.FirstOrDefault()
            : _todasContas.FirstOrDefault();

        foreach (var c in ContasFiltradas)
            c.NotifyIsActiva(ContaSelecionada);

        if (ContaSelecionada is not null)
            await CarregarMovimentosAsync(ContaSelecionada.Id);

        AtualizarGraficoEvolucao();
    }

    private void AtualizarContador()
        => TotalMovimentosTexto = $"{MovimentosFiltrados.Count} movimento(s)";

    private void AtualizarGraficoEvolucao()
    {
        if (ContaSelecionada is null) return;

        var rng    = new Random(ContaSelecionada.Banco.GetHashCode());
        var baseV  = ContaSelecionada.SaldoAtual / 1_000_000;
        var values = Enumerable.Range(0, 12)
                               .Select(_ => Math.Round(baseV * (0.7 + rng.NextDouble() * 0.6), 2))
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

    // ── Mapeamento DTO → Item ─────────────────────────────────────────────────

    private static ContaBancariaItem MapDto(ContaBancariaDto dto) =>
        new(dto.Id, dto.Banco, dto.NIB, dto.Tipo, dto.Moeda,
            (double)dto.SaldoAtual, (double)dto.SaldoOntem,
            dto.Agencia, dto.Titular, dto.CorAccent);

    private static MovimentoBancarioItem MapMovimento(MovimentoBancarioDto m) =>
        new(m.Data.ToString("dd/MM/yyyy"), m.Banco, m.Descricao,
            m.Referencia, m.Tipo, (double)m.Valor, m.Data);
}

// ══════════════════════════════════════════════════════════════════════════════
// ContaBancariaEditavel — versão mutável para o formulário do modal
// ══════════════════════════════════════════════════════════════════════════════

public partial class ContaBancariaEditavel : ObservableObject
{
    public int Id { get; }

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
        Id          = item.Id;
        _banco      = item.Banco;
        _nIB        = item.NIB;
        _tipo       = item.Tipo;
        _moeda      = item.Moeda;
        _agencia    = item.Agencia;
        _titular    = item.Titular;
        _corAccent  = item.CorAccent;
        _saldoAtual = item.SaldoAtual;
        _iniciais   = item.Iniciais;
    }

    public ContaBancariaEditavel(
        int id, string banco, string NIB, string tipo, string moeda,
        string agencia, string titular, string corAccent, double saldoAtual)
    {
        Id          = id;
        _banco      = banco;
        _nIB        = NIB;
        _tipo       = tipo;
        _moeda      = moeda;
        _agencia    = agencia;
        _titular    = titular;
        _corAccent  = corAccent;
        _saldoAtual = saldoAtual;
        _iniciais   = CalcIniciais(banco);
    }

    public void RefreshIniciais() => Iniciais = CalcIniciais(Banco);

    private static string CalcIniciais(string banco) =>
        string.Concat(banco.Split(' ').Where(w => w.Length > 0).Take(2).Select(w => w[0]));

    public string SaldoFmt => $"{SaldoAtual:N0} {Moeda}";
}

// ══════════════════════════════════════════════════════════════════════════════
// ContaBancariaItem — record imutável para a lista principal
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Item da lista principal de contas. Implementa INotifyPropertyChanged via
/// ObservableObject para que a propriedade <see cref="IsActiva"/> possa ser
/// atualizada quando a seleção muda no ViewModel pai.
/// </summary>
public partial class ContaBancariaItem : ObservableObject
{
    public int    Id        { get; }
    public string Banco     { get; }
    public string NIB       { get; }
    public string Tipo      { get; }
    public string Moeda     { get; }
    public double SaldoAtual { get; }
    public double SaldoOntem { get; }
    public string Agencia   { get; }
    public string Titular   { get; }
    public string CorAccent { get; }

    [ObservableProperty] private bool _isActiva;

    public ContaBancariaItem(
        int id, string banco, string nib, string tipo, string moeda,
        double saldoAtual, double saldoOntem,
        string agencia, string titular, string corAccent)
    {
        Id        = id;
        Banco     = banco;
        NIB       = nib;
        Tipo      = tipo;
        Moeda     = moeda;
        SaldoAtual = saldoAtual;
        SaldoOntem = saldoOntem;
        Agencia   = agencia;
        Titular   = titular;
        CorAccent = corAccent;
    }

    /// <summary>Chamado pelo ViewModel pai sempre que a seleção muda.</summary>
    public void NotifyIsActiva(ContaBancariaItem? seleccionada)
        => IsActiva = ReferenceEquals(this, seleccionada);

    public string SaldoFmt      => $"{SaldoAtual:N0} {Moeda}";
    public double Variacao      => SaldoAtual - SaldoOntem;
    public string VariacaoFmt   => Variacao >= 0 ? $"+{Variacao:N0} {Moeda}" : $"-{Math.Abs(Variacao):N0} {Moeda}";
    public string CorVariacao   => Variacao >= 0 ? "#43A047" : "#E53935";
    public string FundoVariacao => Variacao >= 0 ? "#E8F5E9"  : "#FFEBEE";
    public string Iniciais      => string.Concat(Banco.Split(' ').Where(w => w.Length > 0).Take(2).Select(w => w[0]));
}

// ══════════════════════════════════════════════════════════════════════════════
// MovimentoBancarioItem
// ══════════════════════════════════════════════════════════════════════════════

public record MovimentoBancarioItem(
    string Data, string Banco, string Descricao,
    string Referencia, string Tipo, double Valor, DateTime DataOrigem)
{
    public bool   IsCredito => Tipo == "Crédito";
    public bool   IsDebito  => Tipo == "Débito";
    public string ValorFmt  => IsCredito ? $"+{Valor:N0} Kzs" : $"-{Valor:N0} Kzs";
    public string CorValor  => IsCredito ? "#43A047" : "#E53935";
    public string FundoPill => IsCredito ? "#E8F5E9"  : "#FFEBEE";
    public string CorPill   => IsCredito ? "#43A047"  : "#E53935";
}
