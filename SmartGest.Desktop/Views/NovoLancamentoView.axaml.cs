using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class NovoLancamentoView : Window
{
    public NovoLancamentoView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Factory: cria o modal, injeta o OwnerWindow no VM para o file picker
    /// e subscreve o evento de fecho automático.
    /// </summary>
    public static NovoLancamentoView Create(Window owner)
    {
        var vm  = new NovoLancamentoViewModel();
        var win = new NovoLancamentoView { DataContext = vm };

        // O OwnerWindow é necessário para o StorageProvider (file picker)
        vm.OwnerWindow  = win;
        vm.DialogClosed += () => win.Close();

        return win;
    }
}