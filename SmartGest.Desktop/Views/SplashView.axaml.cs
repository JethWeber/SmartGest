using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class SplashView : Window
{
    public SplashView()
    {
        InitializeComponent();
        DataContext ??= App.Services?.GetService<SplashViewModel>() ?? new SplashViewModel();
    }
}
