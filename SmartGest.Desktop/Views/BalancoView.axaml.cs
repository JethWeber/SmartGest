using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class BalancoView : UserControl
{
    public BalancoView()
    {
        InitializeComponent();
        DataContext ??= new BalancoViewModel();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is BalancoViewModel vm)
                vm.OwnerWindow = TopLevel.GetTopLevel(this) as Window;
        };
    }

    protected override async void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is BalancoViewModel vm)
        {
            vm.OwnerWindow = TopLevel.GetTopLevel(this) as Window;
            await vm.InicializarAsync();
        }
    }
}