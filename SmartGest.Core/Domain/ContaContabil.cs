namespace SmartGest.Core.Domain;

public sealed class ContaContabil
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string Grupo { get; private set; } = string.Empty;
    public bool IsDevedora { get; private set; }
    public bool Activa { get; private set; }
    public bool? Corrente { get; private set; }

    private ContaContabil() { }

    public ContaContabil(string codigo, string nome, string grupo, bool isDevedora = true, bool? corrente = null)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("O código é obrigatório.", nameof(codigo));
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("O nome é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(grupo)) throw new ArgumentException("O grupo é obrigatório.", nameof(grupo));

        Codigo = codigo.Trim();
        Nome = nome.Trim();
        Grupo = grupo.Trim();
        IsDevedora = isDevedora;
        Corrente = corrente;
        Activa = true;
    }
}
