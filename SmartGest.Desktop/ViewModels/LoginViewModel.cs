using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    // ── Dependências ──────────────────────────────────────────────────────────
    private readonly AuthService _authService;

    // ── Propriedades ──────────────────────────────────────────────────────────
    [ObservableProperty] private string _phone        = string.Empty;
    [ObservableProperty] private string _password     = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasError     = false;
    [ObservableProperty] private bool   _isLoading    = false;

    // ── Evento de navegação ───────────────────────────────────────────────────
    /// <summary>Disparado na UI thread quando a autenticação é bem-sucedida.</summary>
    public event Action? LoginSucceeded;

    // ── Construtores ──────────────────────────────────────────────────────────

    /// <summary>Construtor principal — usado pelo DI em produção.</summary>
    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Construtor sem parâmetros — usado pelo Avalonia Designer.
    /// Em produção NUNCA é chamado directamente; o DI usa o construtor acima.
    /// </summary>
    public LoginViewModel() : this(
        new AuthService(
            new ApiClient(new TokenStore()),
            new TokenStore()))
    { }

    // ── Comandos ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoginAsync()
    {
        HasError     = false;
        ErrorMessage = string.Empty;

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
            await _authService.LoginAsync(Phone.Trim(), Password);

            // Navega na UI thread (pode ser chamado de uma thread de background)
            await Dispatcher.UIThread.InvokeAsync(() => LoginSucceeded?.Invoke());
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            SetError("Número ou senha incorretos. Tente novamente.");
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            SetError("Utilizador não encontrado ou conta inactiva.");
        }
        catch (ApiException ex)
        {
            SetError($"Erro do servidor ({(int)ex.StatusCode}): {ex.Message}");
        }
        catch (HttpRequestException)
        {
            SetError("Sem ligação à API. Verifique se o servidor está activo.");
        }
        catch (Exception ex)
        {
            SetError($"Erro inesperado: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private void SetError(string msg) { ErrorMessage = msg; HasError = true; }
}
