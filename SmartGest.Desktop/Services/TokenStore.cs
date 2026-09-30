namespace SmartGest.Desktop.Services;

public class TokenStore
{
    public string Token { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string Iniciais { get; set; } = string.Empty;
    public string CorAvatar { get; set; } = "#1A2E5A";

    public bool EstaAutenticado => !string.IsNullOrEmpty(Token);

    public void Limpar()
    {
        Token = string.Empty;
        Nome = string.Empty;
        Telefone = string.Empty;
        Perfil = string.Empty;
        Iniciais = string.Empty;
        CorAvatar = "#1A2E5A";
    }
}