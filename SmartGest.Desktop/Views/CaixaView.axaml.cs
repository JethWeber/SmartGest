using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop.Views;

public partial class CaixaView : UserControl
{
    public CaixaView()
    {
        InitializeComponent();
        DataContext ??= App.Services?.GetService<CaixaViewModel>() ?? new CaixaViewModel();

        // Garantir que o botão abre o modal mesmo que o binding do comando falhe
        BtnNovaTransacao.Click += async (_, __) => await OnNovaTransacaoClicked();
    }

    private async Task OnNovaTransacaoClicked()
    {
        try
        {
            var factory = App.Services?.GetService<Func<NovoLancamentoViewModel>>();
            if (factory is null) return;

            var vm = factory();

            // Quando criado, pedir ao ViewModel de Caixa para recarregar
            vm.LancamentoCriado += resp =>
            {
                if (DataContext is CaixaViewModel caixaVm)
                    _ = Task.Run(async () => await caixaVm.OnLancamentoCriadoAsync());
            };

            var mainWindow = TopLevel.GetTopLevel(this) as Window;
            var dialog = new NovoLancamentoView { DataContext = vm };
            if (mainWindow is not null)
                await dialog.ShowDialog(mainWindow);
        }
        catch
        {
            // não propagar — falha silenciosa (UI não bloqueada)
        }
    }
}