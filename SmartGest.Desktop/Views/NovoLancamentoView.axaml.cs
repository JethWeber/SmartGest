using Avalonia.Controls;
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
                // Fechar a janela quando o VM pede (Cancelar ou sucesso após 1,5 s)
                vm.DialogClosed += Close;

                // Fornece a referência da janela ao VM para o file picker
                vm.OwnerWindow = this;
            }
        };
    }
}
