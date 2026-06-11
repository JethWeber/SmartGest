using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class DreView : UserControl
{
    public DreView()
    {
        InitializeComponent();
        DataContext ??= new DreViewModel();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is DreViewModel vm)
                vm.OwnerWindow = TopLevel.GetTopLevel(this) as Window;
        };
    }

    protected override async void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is DreViewModel vm)
        {
            vm.OwnerWindow = TopLevel.GetTopLevel(this) as Window;
            await vm.InicializarAsync();
        }
    }
}