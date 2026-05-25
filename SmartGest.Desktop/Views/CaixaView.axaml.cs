using Avalonia.Controls;
using SmartGest.Desktop.ViewModels;
 
namespace SmartGest.Desktop.Views;
 
public partial class CaixaView : UserControl
{
    public CaixaView()
    {
        InitializeComponent();
        DataContext ??= new CaixaViewModel();
    }
 
    /// <summary>
    /// Chamado pelo CaixaViewModel via evento OpenNovoLancamento.
    /// Abre o modal sem bloquear a janela principal (ShowDialog = modal sobre a owner).
    /// </summary>
    private void OpenNovoLancamento()
    {
        // Sobe na árvore visual até encontrar a Window (MainWindow)
        var owner = TopLevel.GetTopLevel(this) as Window;
        var modal = NovoLancamentoView.Create(owner!);
        modal.ShowDialog(owner!);   // ShowDialog mantém o foco no modal mas não congela a UI
    }
 
    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
 
        if (DataContext is CaixaViewModel vm)
            vm.OpenNovoLancamento += OpenNovoLancamento;
    }
}
 
