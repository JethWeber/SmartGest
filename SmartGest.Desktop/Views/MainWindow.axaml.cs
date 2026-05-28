using Avalonia.Controls;
using Avalonia.Input;

namespace SmartGest.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Liga os eventos de hover da sidebar ao overlay
        Sidebar.PointerEntered += OnSidebarPointerEntered;
        Sidebar.PointerExited  += OnSidebarPointerExited;

        // NÃO subscrever PedirAbrirNovoLancamento aqui.
        //
        // O App.axaml.cs já subscreve este evento depois de criar o MainWindow:
        //
        //   mainVm.PedirAbrirNovoLancamento += async vm => {
        //       var dialog = new NovoLancamentoView { DataContext = vm };
        //       await dialog.ShowDialog(main);
        //   };
        //
        // Subscrever uma segunda vez aqui causaria DOIS modais por clique —
        // que era exactamente o bug reportado.
    }

    private void OnSidebarPointerEntered(object? sender, PointerEventArgs e)
        => SidebarOverlay.Opacity = 0.35;

    private void OnSidebarPointerExited(object? sender, PointerEventArgs e)
        => SidebarOverlay.Opacity = 0;
}
