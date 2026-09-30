namespace SmartGest.Application.Lancamentos;

public sealed record CriarLancamentoCommand(
    DateTime Data,
    string Descricao,
    string Tipo,
    decimal Valor,
    int CategoriaId,
    string? Beneficiario = null,
    string? MetodoPagamento = null,
    string? CaminhoDocumento = null,
    string? Observacoes = null,
    string? CentroCusto = null,
    string? ReferenciaInterna = null,
    int? ContaBancariaId = null);
