namespace SmartGest.Core.Domain;

public sealed class LancamentoDetalhe
{
    public int Id { get; private set; }
    public int LancamentoId { get; private set; }
    public int ContaContabilId { get; private set; }
    public decimal Debito { get; private set; }
    public decimal Credito { get; private set; }

    private LancamentoDetalhe() { }

    public LancamentoDetalhe(int lancamentoId, int contaContabilId, decimal debito, decimal credito)
    {
        if (lancamentoId <= 0) throw new ArgumentOutOfRangeException(nameof(lancamentoId));
        if (contaContabilId <= 0) throw new ArgumentOutOfRangeException(nameof(contaContabilId));
        if (debito < 0 || credito < 0) throw new DomainException("O débito e o crédito não podem ser negativos.");
        if (debito > 0 && credito > 0) throw new DomainException("Uma partida não pode ter débito e crédito simultaneamente.");
        if (debito == 0 && credito == 0) throw new DomainException("Uma partida deve ter débito ou crédito.");

        LancamentoId = lancamentoId;
        ContaContabilId = contaContabilId;
        Debito = debito;
        Credito = credito;
    }
}
