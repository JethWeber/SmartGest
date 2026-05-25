using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartGest.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _usuarioNome = "Augusto Barbosa";

    /// <summary>
    /// Controla dinamicamente o ecrã exibido no ContentControl.
    /// </summary>
    [ObservableProperty]
    private ViewModelBase _currentPage;

    private readonly CaixaViewModel _caixaVm;
    private readonly BalanceteViewModel _balanceteVm = new();
    private readonly BalancoViewModel _balancoVm = new();
    private readonly DreViewModel _dreVm = new();
    private readonly ContaseBancosViewModel _contasBancosVm = new();
    private readonly ConfiguracoesViewModel _configVm = new();
    

    /// <summary>
    /// Índice do item seleccionado no menu lateral.
    /// O binding two-way com o ListBox.SelectedIndex aciona OnSelectedMenuIndexChanged
    /// sempre que o utilizador clica num item — sem precisar de Commands no ListBoxItem.
    /// </summary>
    [ObservableProperty]
    private int _selectedMenuIndex = 0;

    // ViewModels mantidos em cache para não perder estado ao alternar de aba
    private readonly DashboardViewModel _dashboardVm;

    public MainWindowViewModel()
    {
        _dashboardVm = new DashboardViewModel();
        _caixaVm = new CaixaViewModel();
        
        // Dashboard é a página inicial
        _currentPage = _dashboardVm;
    }

    /// <summary>
    /// Chamado automaticamente pelo CommunityToolkit sempre que SelectedMenuIndex muda.
    /// Cada case corresponde à posição do ListBoxItem no AXAML (começa em 0).
    /// </summary>
    partial void OnSelectedMenuIndexChanged(int value)
    {
        CurrentPage = value switch 
        {
            0 => _dashboardVm,
            // Quando criares os outros ViewModels, substitui os comentários:
            1 => _caixaVm,
            2 => _balanceteVm,
            3 => _balancoVm,
            4 => _dreVm,
            5 => _contasBancosVm,
            6 => _configVm,
            _ => _dashboardVm
        };
    }
}