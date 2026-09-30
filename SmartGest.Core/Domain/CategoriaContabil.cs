namespace SmartGest.Core.Domain;

public sealed class CategoriaContabil
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Tipo { get; private set; }
    public string ContaDebito { get; private set; }
    public string ContaCredito { get; private set; }
    public string GrupoDre { get; private set; }
    public string GrupoBalanco { get; private set; }
    public string GrupoFluxoCaixa { get; private set; }
    public bool AplicaImpostoSelo { get; private set; }
    public bool Ativo { get; private set; }

    private CategoriaContabil() { }

    public CategoriaContabil(string nome, string tipo, string contaDebito, string contaCredito)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("O nome é obrigatório.", nameof(nome));
        if (tipo is not ("Entrada" or "Saída")) throw new ArgumentException("O tipo deve ser Entrada ou Saída.", nameof(tipo));
        if (string.IsNullOrWhiteSpace(contaDebito)) throw new ArgumentException("A conta de débito é obrigatória.", nameof(contaDebito));
        if (string.IsNullOrWhiteSpace(contaCredito)) throw new ArgumentException("A conta de crédito é obrigatória.", nameof(contaCredito));

        Nome = nome.Trim();
        Tipo = tipo;
        ContaDebito = contaDebito.Trim();
        ContaCredito = contaCredito.Trim();
        GrupoDre = "";
        GrupoBalanco = "";
        GrupoFluxoCaixa = "";
        Ativo = true;
    }
}
