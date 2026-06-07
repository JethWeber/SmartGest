using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.ViewModels;

public partial class NovoLancamentoViewModel : ViewModelBase
{
    // ── Dependências ──────────────────────────────────────────────────────────
    private readonly LancamentoService      _lancamentoSvc;
    private readonly ContasBancariasService _contasSvc;
    private readonly CategoriaService       _categoriaSvc;

    // ── Eventos ───────────────────────────────────────────────────────────────
    public event Action? DialogClosed;
    public event Action<LancamentoService.LancamentoResponse>? LancamentoCriado;

    // ── Owner (para o file picker) ────────────────────────────────────────────
    public Window? OwnerWindow { get; set; }

    // ════════════════════════════════════════════════════════════════════════
    // SECÇÃO 1 · MOVIMENTO
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEntrada))]
    [NotifyPropertyChangedFor(nameof(IsSaida))]
    [NotifyPropertyChangedFor(nameof(CorEntrada))]
    [NotifyPropertyChangedFor(nameof(CorSaida))]
    [NotifyPropertyChangedFor(nameof(FundoCirculoEntrada))]
    [NotifyPropertyChangedFor(nameof(FundoCirculoSaida))]
    private int _tipoMovimento = 0;

    public bool IsEntrada => TipoMovimento == 0;
    public bool IsSaida   => TipoMovimento == 1;

    public string CorEntrada          => IsEntrada ? "#43A047" : "#A0A9B8";
    public string CorSaida            => IsSaida   ? "#E53935" : "#A0A9B8";
    public string FundoCirculoEntrada => IsEntrada ? "#D4EDDA" : "#EEF2F7";
    public string FundoCirculoSaida   => IsSaida   ? "#FDECEA" : "#EEF2F7";

    // Recarrega categorias quando o tipo muda
    partial void OnTipoMovimentoChanged(int value) => _ = CarregarCategoriasAsync();

    [ObservableProperty] private string          _valor          = string.Empty;
    [ObservableProperty] private DateTimeOffset? _dataMovimento  = DateTimeOffset.Now;
    [ObservableProperty] private string          _descricao      = string.Empty;
    [ObservableProperty] private int             _categoriaIndex = -1;

    // Categorias carregadas da API (filtradas por tipo)
    [ObservableProperty] private ObservableCollection<string> _categorias = new();

    // ════════════════════════════════════════════════════════════════════════
    // SECÇÃO 2 · FINANCEIRO
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private int    _contaOrigemIndex     = -1;
    [ObservableProperty] private int    _metodoPagamentoIndex = -1;
    [ObservableProperty] private string _beneficiario         = string.Empty;

    public ObservableCollection<ContaItemVm> ContasOrigem { get; } = new();

    public ObservableCollection<string> MetodosPagamento { get; } = new()
    {
        "Transferência Bancária", "Multicaixa / TPA", "Numerário (Dinheiro)", "Cheque",
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemDocumento))]
    [NotifyPropertyChangedFor(nameof(NomeDocumento))]
    [NotifyPropertyChangedFor(nameof(InfoDocumento))]
    [NotifyPropertyChangedFor(nameof(CorBordaDoc))]
    [NotifyPropertyChangedFor(nameof(CorTextoDoc))]
    private string _caminhoDocumento = string.Empty;

    public bool   TemDocumento  => !string.IsNullOrEmpty(CaminhoDocumento);
    public string NomeDocumento => TemDocumento ? Path.GetFileName(CaminhoDocumento) : "Clique para seleccionar ficheiro…";
    public string InfoDocumento => TemDocumento
        ? $"{FormatarTamanho(new FileInfo(CaminhoDocumento).Length)}  ·  {Path.GetExtension(CaminhoDocumento).ToUpper().TrimStart('.')}"
        : string.Empty;
    public string CorBordaDoc => TemDocumento ? "#1A73E8" : "#DDE3EE";
    public string CorTextoDoc => TemDocumento ? "#1A2E5A" : "#A0A9B8";

    // ════════════════════════════════════════════════════════════════════════
    // SECÇÃO 3 · EXTRAS
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private string _observacoes       = string.Empty;
    [ObservableProperty] private string _contadorObs       = "0 / 500";
    [ObservableProperty] private int    _centroCustoIndex  = -1;
    [ObservableProperty] private string _referenciaInterna = string.Empty;

    partial void OnObservacoesChanged(string value)
        => ContadorObs = $"{Math.Min(value.Length, 500)} / 500";

    // Centros de custo — organizacionais, não existem na BD
    public ObservableCollection<string> CentrosCusto { get; } = new()
    {
        "Administração Geral", "Comercial / Vendas", "Produção / Operações",
        "Logística", "Recursos Humanos", "Tecnologia & Sistemas", "Marketing",
    };

    // ════════════════════════════════════════════════════════════════════════
    // ESTADO
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private string _erroMensagem     = string.Empty;
    [ObservableProperty] private bool   _temErro          = false;
    [ObservableProperty] private bool   _isLoading        = false;
    [ObservableProperty] private bool   _contasCarregando = false;
    [ObservableProperty] private string _erroCarregamento = string.Empty;
    [ObservableProperty] private bool   _isSucesso        = false;
    [ObservableProperty] private string _sucessoMensagem  = string.Empty;

    // ════════════════════════════════════════════════════════════════════════
    // CONSTRUTORES
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Construtor principal — DI em produção.</summary>
    public NovoLancamentoViewModel(
        LancamentoService      lancamentoSvc,
        ContasBancariasService contasSvc,
        CategoriaService       categoriaSvc)
    {
        _lancamentoSvc = lancamentoSvc;
        _contasSvc     = contasSvc;
        _categoriaSvc  = categoriaSvc;
        _ = CarregarTudoAsync();
    }

    /// <summary>Construtor sem parâmetros — APENAS para o Avalonia Designer.</summary>
    public NovoLancamentoViewModel()
    {
        var stubStore  = new TokenStore();
        var stubApi    = new ApiClient(stubStore);
        _lancamentoSvc = new LancamentoService(stubApi);
        _contasSvc     = new ContasBancariasService(stubApi);
        _categoriaSvc  = new CategoriaService(stubApi);

        // Dados stub para o designer
        ContasOrigem.Add(new ContaItemVm(1, "Banco BIC  ·  4.820.000 Kzs"));
        ContasOrigem.Add(new ContaItemVm(2, "Banco BAI  ·  3.150.000 Kzs"));
        ContasOrigem.Add(new ContaItemVm(3, "Banco BPC  ·  2.980.000 Kzs"));
        Categorias = new ObservableCollection<string>
        {
            "Venda De Produto", "Prestação De Serviços", "Recebimento De Cliente"
        };
    }

    // ════════════════════════════════════════════════════════════════════════
    // CARREGAMENTO
    // ════════════════════════════════════════════════════════════════════════

    private async Task CarregarTudoAsync()
    {
        await Task.WhenAll(CarregarContasAsync(), CarregarCategoriasAsync());
    }

    private async Task CarregarContasAsync()
    {
        ContasCarregando = true;
        ErroCarregamento = string.Empty;
        try
        {
            var resp = await _contasSvc.ListarAsync();
            ContasOrigem.Clear();
            foreach (var c in resp?.Contas ?? new())
                ContasOrigem.Add(new ContaItemVm(c.Id, $"{c.Banco}  ·  {c.SaldoAtual:N0} {c.Moeda}"));
        }
        catch (Exception ex)
        {
            ErroCarregamento = $"Não foi possível carregar as contas: {ex.Message}";
        }
        finally
        {
            ContasCarregando = false;
        }
    }

    private async Task CarregarCategoriasAsync()
    {
        CategoriaIndex = -1;
        try
        {
            var tipo  = IsEntrada ? "Entrada" : "Saída";
            var lista = await _categoriaSvc.ListarAsync(tipo);
            Categorias = new ObservableCollection<string>(lista.Select(c => c.Nome));
        }
        catch
        {
            Categorias = new ObservableCollection<string>();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // COMANDOS
    // ════════════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void SelecionarTipo(string tipo) => TipoMovimento = tipo == "Saida" ? 1 : 0;

    [RelayCommand]
    private async Task EscolherDocumentoAsync()
    {
        if (OwnerWindow is null) return;
        var topLevel = TopLevel.GetTopLevel(OwnerWindow);
        if (topLevel is null) return;

        var ficheiros = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title          = "Seleccionar Documento / Recibo",
                AllowMultiple  = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Documentos")
                    {
                        Patterns  = new[] { "*.pdf", "*.jpg", "*.jpeg", "*.png" },
                        MimeTypes = new[] { "application/pdf", "image/jpeg", "image/png" }
                    },
                    FilePickerFileTypes.All
                }
            });

        if (ficheiros.Count > 0)
            CaminhoDocumento = ficheiros[0].Path.LocalPath;
    }

    [RelayCommand]
    private void LimparDocumento() => CaminhoDocumento = string.Empty;

    [RelayCommand(CanExecute = nameof(PodeCancelar))]
    private void Cancelar() => DialogClosed?.Invoke();

    private bool PodeCancelar() => !IsLoading;

    [RelayCommand]
    private async Task SalvarLancamentoAsync()
    {
        TemErro      = false;
        ErroMensagem = string.Empty;

        // ── Validações ────────────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(Valor))
        { SetErro("O campo Valor é obrigatório."); return; }

        if (!TryParseValor(Valor, out decimal valorDecimal) || valorDecimal <= 0)
        { SetErro("Valor inválido. Use apenas números (ex: 150000)."); return; }

        if (string.IsNullOrWhiteSpace(Descricao))
        { SetErro("O campo Descrição / Histórico é obrigatório."); return; }

        if (CategoriaIndex < 0)
        { SetErro("Seleccione uma Categoria Financeira."); return; }

        if (ContaOrigemIndex < 0)
        { SetErro("Seleccione a Conta de Origem."); return; }

        if (MetodoPagamentoIndex < 0)
        { SetErro("Seleccione o Método de Pagamento."); return; }

        // ── Envio ─────────────────────────────────────────────────────────────
        IsLoading = true;
        try
        {
            int? contaId = ContaOrigemIndex >= 0 && ContaOrigemIndex < ContasOrigem.Count
                ? ContasOrigem[ContaOrigemIndex].Id
                : null;

            var req = new LancamentoService.LancamentoRequest(
                Data:              DataMovimento?.DateTime ?? DateTime.Today,
                Descricao:         Descricao.Trim(),
                Categoria:         CategoriaIndex >= 0 ? Categorias[CategoriaIndex] : string.Empty,
                Tipo:              IsEntrada ? "Entrada" : "Saída",
                Valor:             valorDecimal,
                Beneficiario:      Beneficiario.Trim(),
                MetodoPagamento:   MetodoPagamentoIndex >= 0 ? MetodosPagamento[MetodoPagamentoIndex] : string.Empty,
                CaminhoDocumento:  CaminhoDocumento,
                Observacoes:       Observacoes.Length > 500 ? Observacoes[..500] : Observacoes,
                CentroCusto:       CentroCustoIndex >= 0 ? CentrosCusto[CentroCustoIndex] : string.Empty,
                ReferenciaInterna: ReferenciaInterna.Trim(),
                ContaBancariaId:   contaId);

            var criado = await _lancamentoSvc.CriarAsync(req);

            SucessoMensagem = $"Lançamento #{criado.Id} guardado com sucesso!";
            IsSucesso       = true;
            LancamentoCriado?.Invoke(criado);

            await Task.Delay(1500);
            DialogClosed?.Invoke();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        { SetErro("Sessão expirada. Por favor, inicie sessão novamente."); }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        { SetErro($"Dados inválidos: {ex.Message}"); }
        catch (ApiException ex)
        { SetErro($"Erro da API ({(int)ex.StatusCode}): {ex.Message}"); }
        catch (HttpRequestException)
        { SetErro($"Sem ligação à API ({ApiClient.BaseUrl}). Verifique o servidor."); }
        catch (Exception ex)
        { SetErro($"Erro inesperado: {ex.Message}"); }
        finally
        { IsLoading = false; }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetErro(string msg) { ErroMensagem = msg; TemErro = true; }

    private static bool TryParseValor(string input, out decimal result)
    {
        var limpo = input.Trim().Replace(" ", "");
        if (decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-PT"), out result) && result > 0) return true;
        if (decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.InvariantCulture, out result) && result > 0) return true;
        if (decimal.TryParse(limpo.Replace(".", "").Replace(",", ""), out result) && result > 0) return true;
        return false;
    }

    private static string FormatarTamanho(long bytes) =>
        bytes >= 1_048_576 ? $"{bytes / 1_048_576.0:F1} MB"
        : bytes >= 1_024   ? $"{bytes / 1_024.0:F0} KB"
        : $"{bytes} B";
}

// ── DTO interno ───────────────────────────────────────────────────────────────
public record ContaItemVm(int Id, string Label)
{
    public override string ToString() => Label;
}