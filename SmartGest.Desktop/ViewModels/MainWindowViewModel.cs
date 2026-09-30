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
    private readonly SessionSecurityService _sessionSecurity;
    private readonly FirstRunService _firstRunService;

    public UiFeedbackService Feedback { get; }

    [ObservableProperty] private bool _onboardingVisible;
    [ObservableProperty] private int _onboardingStep;
    [ObservableProperty] private string _onboardingTitle = string.Empty;
    [ObservableProperty] private string _onboardingDescription = string.Empty;
    [ObservableProperty] private string _onboardingProgress = string.Empty;
    [ObservableProperty] private string _onboardingAction = "Começar";
    [ObservableProperty] private bool _onboardingIsLast;

    // ── Evento que a View subscreve para abrir o modal ────────────────────────
    public event Action<NovoLancamentoViewModel>? PedirAbrirNovoLancamento;
    public event Action? SessionExpiredRequested;

    public void RegistarActividade() => _sessionSecurity.Touch();

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
        DreViewModel dreVm,
        UiFeedbackService feedback,
        SessionSecurityService sessionSecurity,
        FirstRunService firstRunService)
    {
        _novoLancamentoFactory = novoLancamentoFactory;
        Feedback = feedback;
        _sessionSecurity = sessionSecurity;
        _firstRunService = firstRunService;
        _sessionSecurity.SessionExpired += () => SessionExpiredRequested?.Invoke();

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
        App.Services.GetRequiredService<LancamentoService>(),
        App.Services.GetRequiredService<ContaseBancosViewModel>(),
        App.Services.GetRequiredService<DashboardViewModel>(),
        App.Services.GetRequiredService<BalanceteViewModel>(),
        App.Services.GetRequiredService<BalancoViewModel>(),
        App.Services.GetRequiredService<DreViewModel>(),
        App.Services.GetRequiredService<UiFeedbackService>(),
        App.Services.GetRequiredService<SessionSecurityService>(),
        App.Services.GetRequiredService<FirstRunService>())
    { }

    // ── Onboarding completo da aplicação ─────────────────────────────────────

    public void IniciarOnboarding()
    {
        if (_firstRunService.IsCompleted) return;
        OnboardingStep = 0;
        OnboardingVisible = true;
        AtualizarOnboarding();
    }

    [RelayCommand]
    private void ProximoOnboarding()
    {
        if (OnboardingIsLast)
        {
            FecharOnboarding();
            return;
        }

        OnboardingStep++;
        if (OnboardingStep is >= 1 and <= 6)
            SelectedMenuIndex = OnboardingStep switch
            {
                1 => 0, 2 => 1, 3 => 2, 4 => 5, 5 => 6, 6 => 6, _ => 0
            };
        AtualizarOnboarding();
    }

    [RelayCommand]
    private void SaltarOnboarding() => FecharOnboarding();

    private void FecharOnboarding()
    {
        OnboardingVisible = false;
        _firstRunService.MarkCompleted();
    }

    private void AtualizarOnboarding()
    {
        (OnboardingTitle, OnboardingDescription) = OnboardingStep switch
        {
            0 => ("Bem-vindo ao SmartGest", "Este é um tour rápido pela aplicação. Vamos mostrar onde encontrar as principais áreas, como registar movimentos e onde configurar segurança e preferências."),
            1 => ("Dashboard", "É o centro de controlo do SmartGest. Aqui acompanha os principais indicadores, movimentos recentes, saldos e alertas da empresa."),
            2 => ("Caixa", "Use o Caixa para registar lançamentos e movimentos financeiros do dia a dia. Os lançamentos ficam integrados com a contabilidade."),
            3 => ("Relatórios e contabilidade", "Balancete, Balanço Patrimonial e DRE permitem consultar a posição financeira e o desempenho da empresa."),
            4 => ("Contas e Bancos", "Consulte contas bancárias e movimentos associados para manter a tesouraria organizada e acompanhar os saldos."),
            5 => ("Configurações", "Aqui gere os dados da empresa, utilizadores, aparência, notificações, segurança e integrações da aplicação."),
            6 => ("Segurança", "Dentro de Configurações > Segurança pode alterar a senha, configurar o tempo da sessão, auditoria e outras proteções disponíveis."),
            _ => ("Está pronto", "O tour terminou. Pode navegar livremente pelo SmartGest. Se precisar de voltar a estas informações, consulte Configurações ou o menu da aplicação.")
        };

        OnboardingProgress = OnboardingStep == 0 ? "Introdução" : $"Passo {OnboardingStep} de 6";
        OnboardingAction = OnboardingStep >= 7 ? "Concluir" : OnboardingStep == 6 ? "Concluir tour" : OnboardingStep == 0 ? "Começar" : "Próximo";
        OnboardingIsLast = OnboardingStep >= 6;
    }

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