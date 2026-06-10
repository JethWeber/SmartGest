namespace SmartGest.API.DTOs.Responses;

// ── CATEGORIA ─────────────────────────────────────────────────────────────────

public record CategoriaResponse(
    int    Id,
    string Nome,
    string Tipo,
    string GrupoDre,
    string GrupoBalanco,
    string GrupoFluxoCaixa,
    bool   AplicaImpostoSelo);

// ── FLUXO DE CAIXA ────────────────────────────────────────────────────────────

public record FluxoCaixaLinhaResponse(
    string Descricao,
    string Categoria,
    string Grupo,       // Operacional | Investimento | Financiamento
    DateTime Data,
    decimal Valor,
    bool IsEntrada);

public record FluxoCaixaResponse(
    decimal SaldoInicial,
    decimal SaldoFinal,
    decimal TotalEntradas,
    decimal TotalSaidas,
    List<FluxoCaixaLinhaResponse> Entradas,
    List<FluxoCaixaLinhaResponse> Saidas);
