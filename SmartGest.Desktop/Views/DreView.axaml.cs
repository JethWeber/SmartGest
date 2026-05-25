using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class DreView : UserControl
{
    public DreView()
    {
        InitializeComponent();
        DataContext ??= new DreViewModel();
    }
}
