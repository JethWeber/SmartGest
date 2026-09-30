namespace SmartGest.Core.Domain;

public sealed class MovimentoBancario
{
    public int Id { get; private set; }
    public int ContaBancariaId { get; private set; }
    public DateTime Data { get; private set; }
    public string Descricao { get; private set; }
    public string Referencia { get; private set; }
    public string Tipo { get; private set; }
    public decimal Valor { get; private set; }

    private MovimentoBancario() { }

    public MovimentoBancario(int contaBancariaId, DateTime data, string descricao, string referencia, string tipo, decimal valor)
    {
        if (contaBancariaId <= 0) throw new ArgumentOutOfRangeException(nameof(contaBancariaId));
        if (data == default) throw new ArgumentException("A data é obrigatória.", nameof(data));
        if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("A descrição é obrigatória.", nameof(descricao));
        if (tipo is not ("Crédito" or "Débito")) throw new ArgumentException("O tipo deve ser Crédito ou Débito.", nameof(tipo));
        if (valor <= 0) throw new ArgumentOutOfRangeException(nameof(valor), "O valor deve ser maior que zero.");

        ContaBancariaId = contaBancariaId;
        Data = data;
        Descricao = descricao.Trim();
        Referencia = referencia?.Trim() ?? "";
        Tipo = tipo;
        Valor = valor;
    }
}
