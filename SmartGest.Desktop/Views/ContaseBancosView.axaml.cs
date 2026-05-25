using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class ContaseBancosView : UserControl
{
    public ContaseBancosView()
    {
        InitializeComponent();
        DataContext ??= new ContaseBancosViewModel();
    }
}