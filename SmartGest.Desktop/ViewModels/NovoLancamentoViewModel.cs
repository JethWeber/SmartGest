using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartGest.Desktop.ViewModels;

public partial class NovoLancamentoViewModel : ViewModelBase
{
    // ── Evento de fecho do modal ──────────────────────────────────────────────
    public event Action? DialogClosed;

    // ── Referência à Window (injectada pelo code-behind para o file picker) ───
    public Window? OwnerWindow { get; set; }

    // ════════════════════════════════════════════════════════════════════════
    // SECÇÃO 1 · MOVIMENTO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>0 = Entrada · 1 = Saída</summary>
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

    // Cores reactivas dos botões de tipo
    public string CorEntrada         => IsEntrada ? "#43A047" : "#A0A9B8";
    public string CorSaida           => IsSaida   ? "#E53935" : "#A0A9B8";
    public string FundoCirculoEntrada => IsEntrada ? "#D4EDDA" : "#EEF2F7";
    public string FundoCirculoSaida   => IsSaida   ? "#FDECEA" : "#EEF2F7";

    [ObservableProperty] private string          _valor        = string.Empty;
    [ObservableProperty] private DateTimeOffset? _dataMovimento = DateTimeOffset.Now;
    [ObservableProperty] private string          _descricao    = string.Empty;
    [ObservableProperty] private int             _categoriaIndex = -1;

    public ObservableCollection<string> Categorias { get; } = new()
    {
        "Venda de produto",
        "Recebimento de cliente",
        "Adiantamento recebido",
        "Compra de matéria-prima",
        "Material de Escritório",
        "Despesas gerais",
        "Pagamento de salários",
        "Pagamento a fornecedor",
        "Encargos bancários",
        "Outros",
    };

    // ════════════════════════════════════════════════════════════════════════
    // SECÇÃO 2 · FINANCEIRO
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private int    _contaOrigemIndex      = -1;
    [ObservableProperty] private int    _metodoPagamentoIndex  = -1;
    [ObservableProperty] private string _beneficiario          = string.Empty;

    public ObservableCollection<string> ContasOrigem { get; } = new()
    {
        "BAI - Conta Corrente  ·  Saldo: 2.450.000 Kz",
        "BPC - Conta Poupança  ·  Saldo: 800.000 Kz",
        "ATL 021               ·  Saldo: 320.000 Kz",
        "Caixa Física          ·  Saldo: 150.000 Kz",
    };

    public ObservableCollection<string> MetodosPagamento { get; } = new()
    {
        "Transferência Bancária",
        "Multicaixa / TPA",
        "Numerário (Dinheiro)",
        "Cheque",
    };

    // ── Documento / Recibo (file picker) ─────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemDocumento))]
    [NotifyPropertyChangedFor(nameof(NomeDocumento))]
    [NotifyPropertyChangedFor(nameof(InfoDocumento))]
    [NotifyPropertyChangedFor(nameof(CorBordaDoc))]
    [NotifyPropertyChangedFor(nameof(CorTextoDoc))]
    private string _caminhoDocumento = string.Empty;

    public bool   TemDocumento  => !string.IsNullOrEmpty(CaminhoDocumento);
    public string NomeDocumento => TemDocumento
        ? Path.GetFileName(CaminhoDocumento)
        : "Clique para seleccionar ficheiro…";
    public string InfoDocumento => TemDocumento
        ? $"{FormatarTamanho(new FileInfo(CaminhoDocumento).Length)}  ·  {Path.GetExtension(CaminhoDocumento).ToUpper().TrimStart('.')}"
        : string.Empty;
    public string CorBordaDoc   => TemDocumento ? "#1A73E8" : "#DDE3EE";
    public string CorTextoDoc   => TemDocumento ? "#1A2E5A" : "#A0A9B8";

    private static string FormatarTamanho(long bytes) =>
        bytes >= 1_048_576 ? $"{bytes / 1_048_576.0:F1} MB"
        : bytes >= 1_024   ? $"{bytes / 1_024.0:F0} KB"
        : $"{bytes} B";

    // ════════════════════════════════════════════════════════════════════════
    // SECÇÃO 3 · EXTRAS
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private string _observacoes      = string.Empty;
    [ObservableProperty] private string _contadorObs      = "0 / 500";
    [ObservableProperty] private int    _centroCustoIndex = -1;
    [ObservableProperty] private string _referenciaInterna = string.Empty;

    partial void OnObservacoesChanged(string value)
        => ContadorObs = $"{Math.Min(value.Length, 500)} / 500";

    public ObservableCollection<string> CentrosCusto { get; } = new()
    {
        "Administração Geral",
        "Comercial / Vendas",
        "Produção / Operações",
        "Logística",
        "Recursos Humanos",
        "Tecnologia & Sistemas",
        "Marketing",
    };

    // ════════════════════════════════════════════════════════════════════════
    // VALIDAÇÃO / ESTADO
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private string _erroMensagem = string.Empty;
    [ObservableProperty] private bool   _temErro       = false;
    [ObservableProperty] private bool   _isLoading     = false;

    // ════════════════════════════════════════════════════════════════════════
    // COMANDOS
    // ════════════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void SelecionarTipo(string tipo)
        => TipoMovimento = tipo == "Saida" ? 1 : 0;

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
                        Patterns = new[] { "*.pdf", "*.jpg", "*.jpeg", "*.png" },
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

    [RelayCommand]
    private void Cancelar() => DialogClosed?.Invoke();

    [RelayCommand]
    private async Task SalvarLancamentoAsync()
    {
        TemErro = false;
        ErroMensagem = string.Empty;

        if (string.IsNullOrWhiteSpace(Valor))
        { SetErro("O campo Valor é obrigatório."); return; }

        if (string.IsNullOrWhiteSpace(Descricao))
        { SetErro("O campo Descrição / Histórico é obrigatório."); return; }

        if (CategoriaIndex < 0)
        { SetErro("Seleccione uma Categoria Financeira."); return; }

        if (ContaOrigemIndex < 0)
        { SetErro("Seleccione a Conta de Origem."); return; }

        if (MetodoPagamentoIndex < 0)
        { SetErro("Seleccione o Método de Pagamento."); return; }

        IsLoading = true;
        try
        {
            // TODO: persistir via repositório / API
            await Task.Delay(800);
            DialogClosed?.Invoke();
        }
        catch (Exception ex)
        {
            SetErro($"Erro ao guardar: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private void SetErro(string msg) { ErroMensagem = msg; TemErro = true; }
}