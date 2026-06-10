namespace SmartGest.API.DTOs.Requests;

// ── CATEGORIA ─────────────────────────────────────────────────────────────────

public record CategoriaRequest(
    string  Nome,
    string  Tipo,               // "Entrada" | "Saída"
    string  ContaDebito,
    string  ContaCredito,
    string? GrupoDre,
    string? GrupoBalanco,
    string? GrupoFluxoCaixa,
    bool    AplicaImpostoSelo);
