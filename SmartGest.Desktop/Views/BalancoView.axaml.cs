using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class BalancoView : UserControl
{
    public BalancoView()
    {
        InitializeComponent();
        DataContext ??= new BalancoViewModel();
    }
}
