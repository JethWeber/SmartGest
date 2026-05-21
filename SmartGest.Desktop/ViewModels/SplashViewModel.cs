using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartGest.Desktop.ViewModels;

public partial class SplashViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _statusText = "A iniciar SmartGest...";

    [ObservableProperty]
    private string _progressText = "0%";

    [ObservableProperty]
    private int _progressValue = 0; // 0–100

    public event Action? LoadingCompleted;

    public SplashViewModel()
    {
        _ = RunLoadingSequenceAsync();
    }

    private async Task RunLoadingSequenceAsync()
    {
        var steps = new[]
        {
            (pct: 15,  status: "A verificar configurações..."),
            (pct: 30,  status: "A carregar módulos financeiros..."),
            (pct: 50,  status: "A inicializar motor contábil..."),
            (pct: 70,  status: "A preparar dashboards..."),
            (pct: 85,  status: "A conectar serviços..."),
            (pct: 100, status: "Pronto!"),
        };

        foreach (var step in steps)
        {
            await Task.Delay(380);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressValue = step.pct;
                ProgressText  = $"{step.pct}%";
                StatusText    = step.status;
            });
        }

        await Task.Delay(500);
        await Dispatcher.UIThread.InvokeAsync(() => LoadingCompleted?.Invoke());
    }
}
