using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        // Tooltip formatting left to chart defaults for compatibility across versions
        DataContext ??= App.Services?.GetService<DashboardViewModel>() ?? new DashboardViewModel();
    }
}