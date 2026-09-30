namespace SmartGest.Core.Domain;

public sealed class ContaBancaria
{
    public int Id { get; private set; }
    public string Banco { get; private set; } = string.Empty;
    public string NIB { get; private set; } = string.Empty;
    public string Tipo { get; private set; } = string.Empty;
    public string Moeda { get; private set; } = string.Empty;
    public decimal SaldoAtual { get; private set; }
    public decimal SaldoOntem { get; private set; }
    public string Agencia { get; private set; } = string.Empty;
    public string Titular { get; private set; } = string.Empty;
    public string CorAccent { get; private set; } = string.Empty;
    public bool Activa { get; private set; }
    public int? ContaContabilId { get; private set; }

    private ContaBancaria() { }

    public ContaBancaria(string banco, string nib, string tipo = "Conta à Ordem", string moeda = "AOA")
    {
        if (string.IsNullOrWhiteSpace(banco)) throw new ArgumentException("O banco é obrigatório.", nameof(banco));
        Banco = banco.Trim();
        NIB = nib?.Trim() ?? "";
        Tipo = tipo?.Trim() ?? "Conta à Ordem";
        Moeda = string.IsNullOrWhiteSpace(moeda) ? "AOA" : moeda.Trim();
        Agencia = "";
        Titular = "";
        CorAccent = "#1A2E5A";
        Activa = true;
    }
}
