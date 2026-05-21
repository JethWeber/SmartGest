using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel();
    }
}