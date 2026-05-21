using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartGest.Desktop.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    // ── Propriedades ligadas à View ──────────────────────────────────────────

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError = false;

    [ObservableProperty]
    private bool _isLoading = false;

    // ── Evento de navegação ──────────────────────────────────────────────────

    /// <summary>Disparado quando a autenticação é bem-sucedida.</summary>
    public event Action? LoginSucceeded;

    // ── Comandos ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoginAsync()
    {
        // Reset de erros
        HasError = false;
        ErrorMessage = string.Empty;

        // Validação local
        if (string.IsNullOrWhiteSpace(Phone))
        {
            SetError("Por favor, insira o número de telefone.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            SetError("Por favor, insira a senha de acesso.");
            return;
        }

        IsLoading = true;

        try
        {
            // TODO: substituir pela chamada real ao endpoint /api/auth/login
            await Task.Delay(1400);

            // Credenciais demo para o MVP
            if (Phone == "900000000" && Password == "smartgest")
            {
                await Dispatcher.UIThread.InvokeAsync(() => LoginSucceeded?.Invoke());
            }
            else
            {
                SetError("Número ou senha incorretos. Tente novamente.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Erro de ligação: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }
}
