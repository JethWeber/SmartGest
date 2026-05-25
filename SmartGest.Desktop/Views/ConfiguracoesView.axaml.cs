using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class ConfiguracoesView : UserControl
{
    public ConfiguracoesView()
    {
        InitializeComponent();
        DataContext ??= new ConfiguracoesViewModel();
    }
}
