using Avalonia.Controls;
using Avalonia.Input;

namespace SmartGest.Desktop.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
        var drag = this.FindControl<Border>("DragRegion");
        if (drag is not null)
            drag.PointerPressed += (_, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    BeginMoveDrag(e);
            };
    }
}
