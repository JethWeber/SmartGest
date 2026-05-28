using System;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Serviço de autenticação.
/// IMPORTANTE: deve receber o MESMO TokenStore singleton que o ApiClient.
/// Se o DI estiver configurado correctamente (ambos AddSingleton), isto é
/// garantido automaticamente.
/// </summary>
public class AuthService
{
    private readonly ApiClient  _api;
    private readonly TokenStore _store;

    // O DI injeta o ApiClient singleton E o TokenStore singleton.
    // É o MESMO TokenStore que está dentro do ApiClient — garantido pelo DI.
    public AuthService(ApiClient api, TokenStore store)
    {
        _api   = api;
        _store = store;
    }

    public async Task LoginAsync(string telefone, string password)
    {
        var resp = await _api.PostAsync<LoginResponse>(
            "api/auth/login",
            new { Telefone = telefone, Password = password });

        if (resp is null)
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError,
                "Resposta inválida do servidor.");

        // Escreve no TokenStore — que é o MESMO singleton que o ApiClient usa
        _store.Token     = resp.Token;
        _store.Nome      = resp.Nome;
        _store.Perfil    = resp.Perfil;
        _store.Iniciais  = resp.Iniciais;
        _store.CorAvatar = resp.CorAvatar;

        // Diagnóstico: confirma que o token foi escrito
        System.Diagnostics.Debug.WriteLine(
            $"[AuthService] Token escrito. Primeiros 20 chars: {resp.Token[..Math.Min(20, resp.Token.Length)]}...");
        System.Diagnostics.Debug.WriteLine(
            $"[AuthService] TokenStore.GetHashCode={_store.GetHashCode()}");
    }

    public async Task AlterarSenhaAsync(string senhaAtual, string senhaNova, string senhaConf)
    {
        await _api.PostAsync("api/auth/alterar-senha", new
        {
            SenhaAtual       = senhaAtual,
            SenhaNova        = senhaNova,
            SenhaConfirmacao = senhaConf
        });
    }

    public void Logout() => _store.Limpar();

    private record LoginResponse(
        string Token, string Nome, string Perfil,
        string Iniciais, string CorAvatar);
}
