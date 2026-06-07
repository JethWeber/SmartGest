using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class BalanceteView : UserControl
{
    public BalanceteView()
    {
        InitializeComponent();

        // Se o DataContext já vier injectado pelo MainWindow não recriamos.
        DataContext ??= new BalanceteViewModel();
    }

    protected override async void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is BalanceteViewModel vm)
            await vm.InicializarAsync();
    }
}