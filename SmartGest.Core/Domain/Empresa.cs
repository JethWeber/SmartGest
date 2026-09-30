namespace SmartGest.Core.Domain;

public sealed class Empresa
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string NIF { get; private set; } = string.Empty;
    public string Morada { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Pais { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Website { get; private set; } = string.Empty;
    public decimal Capital { get; private set; }
    public string? LogoPath { get; private set; }

    private Empresa() { }

    public Empresa(string nome, string nif = "", string morada = "", string cidade = "", string pais = "Angola")
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("O nome da empresa é obrigatório.", nameof(nome));
        Nome = nome.Trim();
        NIF = nif?.Trim() ?? "";
        Morada = morada?.Trim() ?? "";
        Cidade = cidade?.Trim() ?? "";
        Pais = string.IsNullOrWhiteSpace(pais) ? "Angola" : pais.Trim();
        Telefone = "";
        Email = "";
        Website = "";
    }
}
