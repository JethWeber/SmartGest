namespace SmartGest.Core.Domain;

public sealed class Lancamento
{
    public int Id { get; private set; }
    public DateTime Data { get; private set; }
    public string Descricao { get; private set; }
    public string Categoria { get; private set; }
    public int? CategoriaContabilId { get; private set; }
    public string Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public string Beneficiario { get; private set; }
    public string MetodoPagamento { get; private set; }
    public string CaminhoDocumento { get; private set; }
    public string Observacoes { get; private set; }
    public string CentroCusto { get; private set; }
    public string ReferenciaInterna { get; private set; }
    public decimal ImpostoSelo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public bool Anulado { get; private set; }
    public DateTime? AnuladoEm { get; private set; }
    public string? AnuladoPor { get; private set; }
    public string? MotivoAnulacao { get; private set; }
    public int? ContaBancariaId { get; private set; }

    private Lancamento() { }

    public Lancamento(DateTime data, string descricao, string tipo, decimal valor, int categoriaId,
        string? beneficiario = null, string? metodoPagamento = null, string? caminhoDocumento = null,
        string? observacoes = null, string? centroCusto = null, string? referenciaInterna = null,
        int? contaBancariaId = null)
    {
        if (data == default) throw new ArgumentException("A data é obrigatória.", nameof(data));
        if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("A descrição é obrigatória.", nameof(descricao));
        if (tipo is not ("Entrada" or "Saída")) throw new ArgumentException("O tipo deve ser Entrada ou Saída.", nameof(tipo));
        if (valor <= 0) throw new ArgumentOutOfRangeException(nameof(valor), "O valor deve ser maior que zero.");
        if (categoriaId <= 0) throw new ArgumentOutOfRangeException(nameof(categoriaId), "A categoria é obrigatória.");

        Data = data;
        Descricao = descricao.Trim();
        Tipo = tipo;
        Valor = valor;
        CategoriaContabilId = categoriaId;
        Beneficiario = beneficiario?.Trim() ?? "";
        MetodoPagamento = metodoPagamento?.Trim() ?? "";
        CaminhoDocumento = caminhoDocumento?.Trim() ?? "";
        Observacoes = (observacoes ?? "").Trim();
        CentroCusto = centroCusto?.Trim() ?? "";
        ReferenciaInterna = referenciaInterna?.Trim() ?? "";
        ContaBancariaId = contaBancariaId;
        Categoria = "";
        CriadoEm = DateTime.UtcNow;
    }

    public void DefinirCategoria(string categoria) => Categoria = categoria?.Trim() ?? "";
    public void Anular(string utilizador, string motivo)
    {
        if (Anulado) throw new InvalidOperationException("O lançamento já está anulado.");
        if (string.IsNullOrWhiteSpace(utilizador)) throw new ArgumentException("O utilizador é obrigatório.", nameof(utilizador));
        if (string.IsNullOrWhiteSpace(motivo)) throw new ArgumentException("O motivo da anulação é obrigatório.", nameof(motivo));
        Anulado = true;
        AnuladoEm = DateTime.UtcNow;
        AnuladoPor = utilizador.Trim();
        MotivoAnulacao = motivo.Trim();
    }
}
