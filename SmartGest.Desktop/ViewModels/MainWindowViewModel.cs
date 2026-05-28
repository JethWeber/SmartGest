using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // ── Dados do utilizador autenticado ───────────────────────────────────────
    [ObservableProperty] private string _usuarioNome      = "Utilizador";
    [ObservableProperty] private string _usuarioIniciais  = "??";
    [ObservableProperty] private string _usuarioCorAvatar = "#1A2E5A";

    /// <summary>Controla o ecrã exibido no ContentControl.</summary>
    [ObservableProperty] private ViewModelBase _currentPage;

    /// <summary>
    /// Índice do item seleccionado no menu lateral.
    /// 0=Dashboard · 1=Caixa · 2=Balancete · 3=Balanço · 4=DRE · 5=Contas · 6=Config
    /// </summary>
    [ObservableProperty] private int _selectedMenuIndex = 0;

    // ── ViewModels em cache (criados uma vez, estado preservado ao navegar) ───
    private readonly DashboardViewModel     _dashboardVm;
    private readonly CaixaViewModel         _caixaVm;
    private readonly BalanceteViewModel     _balanceteVm;
    private readonly BalancoViewModel       _balancoVm;
    private readonly DreViewModel           _dreVm;
    private readonly ContaseBancosViewModel _contasBancosVm;
    private readonly ConfiguracoesViewModel _configVm;

    // ── Factory para NovoLancamentoViewModel (injectada pelo DI) ─────────────
    //
    // PORQUÊ Func<> em vez de injectar NovoLancamentoViewModel directamente?
    //
    //   • NovoLancamentoViewModel é Transient: deve ser criado de novo a cada
    //     abertura do modal, para que os campos comecem limpos.
    //   • Se fosse injectado directamente, o DI criaria UMA instância no
    //     arranque e reutilizá-la-ia sempre — os campos ficariam "sujos" entre
    //     aberturas do modal.
    //   • Func<T> é o padrão idiomático de .NET DI para "fábrica de Transient":
    //     cada chamada a _novoLancamentoFactory() resolve uma nova instância
    //     com as dependências singleton correctas (ApiClient + TokenStore).
    //
    // PORQUÊ não usar new NovoLancamentoViewModel()?
    //
    //   • O construtor sem parâmetros é o construtor de designer: cria um
    //     TokenStore vazio, sem token. Qualquer pedido à API resultaria em 401.
    //   • Só o DI conhece o TokenStore singleton com o token escrito após login.
    private readonly Func<NovoLancamentoViewModel> _novoLancamentoFactory;

    // ── Evento que a View subscreve para abrir o modal ────────────────────────
    /// <summary>
    /// Disparado quando o utilizador pede um novo lançamento.
    /// O argumento é um NovoLancamentoViewModel já correctamente construído
    /// pelo DI — a View apenas abre o modal com este ViewModel como DataContext.
    ///
    /// COMO USAR na View (ex.: MainWindow.axaml.cs):
    ///
    ///   mainVm.PedirAbrirNovoLancamento += async vm =>
    ///   {
    ///       var dialog = new NovoLancamentoView { DataContext = vm };
    ///       vm.OwnerWindow = this;
    ///       await dialog.ShowDialog(this);
    ///   };
    /// </summary>
    public event Action<NovoLancamentoViewModel>? PedirAbrirNovoLancamento;

    // ── Construtor principal (DI) ─────────────────────────────────────────────
    public MainWindowViewModel(
        TokenStore store,
        Func<NovoLancamentoViewModel> novoLancamentoFactory)
    {
        _novoLancamentoFactory = novoLancamentoFactory;

        UsuarioNome      = store.Nome;
        UsuarioIniciais  = store.Iniciais;
        UsuarioCorAvatar = store.CorAvatar;

        _dashboardVm    = new DashboardViewModel();
        _caixaVm        = new CaixaViewModel();
        _balanceteVm    = new BalanceteViewModel();
        _balancoVm      = new BalancoViewModel();
        _dreVm          = new DreViewModel();
        _contasBancosVm = new ContaseBancosViewModel();
        _configVm       = new ConfiguracoesViewModel();

        _currentPage = _dashboardVm;

        // Subscreve o evento do CaixaViewModel aqui, no ViewModel,
        // para que a criação do NovoLancamentoViewModel passe sempre pelo DI.
        _caixaVm.OpenNovoLancamento += AbrirNovoLancamento;
    }

    /// <summary>
    /// Construtor sem parâmetros — usado APENAS pelo Avalonia Designer.
    /// Em produção o DI resolve sempre o construtor com TokenStore.
    /// </summary>
    public MainWindowViewModel() : this(
        new TokenStore
        {
            Nome      = "Augusto Barbosa",
            Iniciais  = "AB",
            CorAvatar = "#1A2E5A"
        },
        // O designer usa o construtor de designer do NovoLancamentoViewModel,
        // que também é seguro para preview (dados stub, sem chamadas à API).
        () => new NovoLancamentoViewModel())
    { }

    // ── Navegação ─────────────────────────────────────────────────────────────

    partial void OnSelectedMenuIndexChanged(int value)
    {
        CurrentPage = value switch
        {
            0 => _dashboardVm,
            1 => _caixaVm,
            2 => _balanceteVm,
            3 => _balancoVm,
            4 => _dreVm,
            5 => _contasBancosVm,
            6 => _configVm,
            _ => _dashboardVm
        };
    }

    // ── Handler interno ───────────────────────────────────────────────────────

    /// <summary>
    /// Cria uma instância fresca de NovoLancamentoViewModel via DI
    /// e notifica a View para abrir o modal.
    /// </summary>
    private void AbrirNovoLancamento()
    {
        var vm = _novoLancamentoFactory();    // Transient: campos limpos + token correcto
        PedirAbrirNovoLancamento?.Invoke(vm); // View abre o modal com este VM
    }
}
