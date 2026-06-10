using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class NovoLancamentoView : Window
{
    public NovoLancamentoView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is NovoLancamentoViewModel vm)
            {
                vm.DialogClosed += Close;
                vm.OwnerWindow = this;
            }
        };

        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
                e.Source is Avalonia.Visual v &&
                v.GetVisualAncestors().OfType<Button>().Any() == false &&
                v.GetVisualAncestors().OfType<TextBox>().Any() == false &&
                v.GetVisualAncestors().OfType<ComboBox>().Any() == false)
            {
                var pos = e.GetPosition(this);
                if (pos.Y < 80)
                    BeginMoveDrag(e);
            }
        };
    }
}
