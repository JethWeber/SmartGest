namespace SmartGest.Application.Lancamentos;

public sealed record LancamentoResult(
    int Id,
    DateTime Data,
    string Descricao,
    string Tipo,
    decimal Valor,
    int CategoriaId,
    string Categoria);
