using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartGest.Desktop.Services;

public enum FeedbackKind { Success, Error, Warning, Info }

public sealed partial class UiFeedbackService : ObservableObject
{
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private FeedbackKind _kind = FeedbackKind.Info;
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private bool _isBusy;
    private CancellationTokenSource? _hideCts;

    public async Task BusyAsync(Func<Task> operation, string? successMessage = null)
    {
        IsBusy = true;
        try
        {
            await operation();
            if (!string.IsNullOrWhiteSpace(successMessage))
                ShowSuccess(successMessage);
        }
        catch (Exception ex)
        {
            ShowError("Não foi possível concluir a operação.", ex);
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ShowSuccess(string message) => Show(message, FeedbackKind.Success);
    public void ShowInfo(string message) => Show(message, FeedbackKind.Info);
    public void ShowWarning(string message) => Show(message, FeedbackKind.Warning);
    public void ShowError(string message, Exception? exception = null)
    {
        if (exception is not null)
            AppLogService.Error(message, exception);
        Show(message, FeedbackKind.Error);
    }

    private void Show(string message, FeedbackKind kind)
    {
        void Apply()
        {
            Message = message;
            Kind = kind;
            IsVisible = true;
        }

        if (Dispatcher.UIThread.CheckAccess()) Apply();
        else Dispatcher.UIThread.Post(Apply);

        _hideCts?.Cancel();
        _hideCts = new CancellationTokenSource();
        _ = HideLaterAsync(_hideCts.Token);
    }

    private async Task HideLaterAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(4), token);
            await Dispatcher.UIThread.InvokeAsync(() => IsVisible = false);
        }
        catch (OperationCanceledException) { }
    }
}
