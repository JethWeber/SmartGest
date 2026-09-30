namespace SmartGest.Core.Domain;

public sealed class Utilizador
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Telefone { get; private set; }
    public string PasswordHash { get; private set; }
    public string Perfil { get; private set; }
    public bool Activo { get; private set; }
    public string Iniciais { get; private set; }
    public string CorAvatar { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Utilizador() { }

    public Utilizador(string nome, string email, string telefone, string passwordHash, string perfil = "Operador")
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("O nome é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("O email é obrigatório.", nameof(email));
        if (string.IsNullOrWhiteSpace(telefone)) throw new ArgumentException("O telefone é obrigatório.", nameof(telefone));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("A password hash é obrigatória.", nameof(passwordHash));

        Nome = nome.Trim();
        Email = email.Trim();
        Telefone = telefone.Trim();
        PasswordHash = passwordHash;
        Perfil = string.IsNullOrWhiteSpace(perfil) ? "Operador" : perfil.Trim();
        Activo = true;
        Iniciais = ObterIniciais(Nome);
        CorAvatar = "#1A2E5A";
        CriadoEm = DateTime.UtcNow;
    }

    private static string ObterIniciais(string nome)
    {
        var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length == 1
            ? partes[0][..Math.Min(2, partes[0].Length)].ToUpperInvariant()
            : $"{partes[0][0]}{partes[^1][0]}".ToUpperInvariant();
    }
}
