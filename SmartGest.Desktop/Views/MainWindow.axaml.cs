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
    }

    private void OnSidebarPointerEntered(object? sender, PointerEventArgs e)
    {
        SidebarOverlay.Opacity = 0.35;
    }

    private void OnSidebarPointerExited(object? sender, PointerEventArgs e)
    {
        SidebarOverlay.Opacity = 0;
    }
}