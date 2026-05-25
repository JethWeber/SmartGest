using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class BalanceteView : UserControl
{
    public BalanceteView()
    {
        InitializeComponent();
        DataContext ??= new BalanceteViewModel();
    }
}
