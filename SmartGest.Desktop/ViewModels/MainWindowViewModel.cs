using System;
using System.Threading.Tasks;
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
    private readonly Func<NovoLancamentoViewModel> _novoLancamentoFactory;

    // ── Evento que a View subscreve para abrir o modal ────────────────────────
    public event Action<NovoLancamentoViewModel>? PedirAbrirNovoLancamento;

    // ── Construtor principal (DI) ─────────────────────────────────────────────
    // ContaseBancosViewModel é injectado pelo DI (Singleton) — garante que usa
    // o mesmo ApiClient/TokenStore com o token JWT preenchido após login.
    public MainWindowViewModel(
        TokenStore store,
        Func<NovoLancamentoViewModel> novoLancamentoFactory,
        LancamentoService lancamentoSvc,
        ContaseBancosViewModel contasBancosVm,
        DashboardViewModel dashboardVm,
        BalanceteViewModel balanceteVm,
        BalancoViewModel balancoVm,
        DreViewModel dreVm)
    {
        _novoLancamentoFactory = novoLancamentoFactory;

        UsuarioNome      = store.Nome;
        UsuarioIniciais  = store.Iniciais;
        UsuarioCorAvatar = store.CorAvatar;

        _dashboardVm    = dashboardVm;
        _caixaVm        = new CaixaViewModel(lancamentoSvc);
        _balanceteVm    = balanceteVm;
        _balancoVm      = balancoVm;
        _dreVm          = dreVm;
        _contasBancosVm = contasBancosVm;
        _configVm       = new ConfiguracoesViewModel();

        _currentPage = _dashboardVm;

        _caixaVm.OpenNovoLancamento += AbrirNovoLancamento;
    }

    /// <summary>Construtor sem parâmetros — usado APENAS pelo Avalonia Designer.</summary>
    public MainWindowViewModel() : this(
        new TokenStore
        {
            Nome      = "Augusto Barbosa",
            Iniciais  = "AB",
            CorAvatar = "#1A2E5A"
        },
        () => new NovoLancamentoViewModel(),
        new LancamentoService(new ApiClient(new TokenStore())),
        new ContaseBancosViewModel(new ContasBancariasService(new ApiClient(new TokenStore()))),
        new DashboardViewModel(new DashboardService(new ApiClient(new TokenStore()))),
        new BalanceteViewModel(new ContabilidadeService(new ApiClient(new TokenStore()))),
        new BalancoViewModel(new ContabilidadeService(new ApiClient(new TokenStore()))),
        new DreViewModel(new ContabilidadeService(new ApiClient(new TokenStore()))))
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

        if (value == 0)
            _ = _dashboardVm.ActivarAsync();
        else if (value == 2)
            _ = _balanceteVm.InicializarAsync();
        else if (value == 3)
            _ = _balancoVm.InicializarAsync();
        else if (value == 4)
            _ = _dreVm.InicializarAsync();
        else if (value == 5)
            _ = _contasBancosVm.ActivarAsync();
    }

    // ── Handler interno ───────────────────────────────────────────────────────

    private void AbrirNovoLancamento()
    {
        var vm = _novoLancamentoFactory();

        // Quando o lançamento for criado com sucesso, recarrega o Caixa
        vm.LancamentoCriado += _ => Task.Run(async () => await _caixaVm.OnLancamentoCriadoAsync());

        PedirAbrirNovoLancamento?.Invoke(vm);
    }
}