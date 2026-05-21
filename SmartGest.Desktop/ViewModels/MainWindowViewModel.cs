using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartGest.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _usuarioNome = "Augusto Barbosa";

    // 🌍 Esta propriedade controla dinamicamente o ecrã exibido no ContentControl
    [ObservableProperty]
    private ViewModelBase _currentPage;

    // ViewModels mantidos em cache para não perder o estado ao alternar de aba
    private readonly DashboardViewModel _dashboardVm;

    public MainWindowViewModel()
    {
        // Instancia os submódulos
        _dashboardVm = new DashboardViewModel();

        // Define o Dashboard como a página inicial padrão da aplicação
        _currentPage = _dashboardVm;
    }

    // ── Comandos de Navegação do Menu ────────────────────────────────────────

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentPage = _dashboardVm;
    }

    [RelayCommand]
    private void NavigateToDre()
    {
        // TODO: Substituir por: CurrentPage = new DreViewModel();
    }

    [RelayCommand]
    private void NavigateToFacturacao()
    {
        // TODO: Substituir por: CurrentPage = new FacturacaoViewModel();
    }
}