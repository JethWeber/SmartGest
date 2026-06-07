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

    protected override async void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is DreViewModel vm)
            await vm.InicializarAsync();
    }
}