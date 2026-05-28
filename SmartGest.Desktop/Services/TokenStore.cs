namespace SmartGest.Desktop.Services;

/// <summary>
/// Singleton que guarda o token JWT e os dados do utilizador autenticado.
/// Injectado via DI em todos os serviços que precisam de autenticação.
/// </summary>
public class TokenStore
{
    public string Token    { get; set; } = string.Empty;
    public string Nome     { get; set; } = string.Empty;
    public string Perfil   { get; set; } = string.Empty;
    public string Iniciais { get; set; } = string.Empty;
    public string CorAvatar{ get; set; } = "#1A2E5A";

    public bool EstaAutenticado => !string.IsNullOrEmpty(Token);

    public void Limpar()
    {
        Token     = string.Empty;
        Nome      = string.Empty;
        Perfil    = string.Empty;
        Iniciais  = string.Empty;
        CorAvatar = "#1A2E5A";
    }
}
