namespace SmartGest.API.Models;

/// <summary>
/// Tabela-mestre de categorias contabilísticas.
/// Define, de forma centralizada e determinística, as contas PGC Angola
/// que cada tipo de operação movimenta.
/// O utilizador nunca escolhe contas — escolhe apenas a Categoria.
/// </summary>
public class CategoriaContabil
{
    public int    Id               { get; set; }
    public string Nome             { get; set; } = string.Empty;

    /// <summary>"Entrada" | "Saída"</summary>
    public string Tipo             { get; set; } = string.Empty;

    // ── Contas PGC (códigos, não IDs) ────────────────────────────────────────
    public string ContaDebito      { get; set; } = string.Empty;
    public string ContaCredito     { get; set; } = string.Empty;

    // ── Classificação para relatórios ─────────────────────────────────────────
    /// <summary>
    /// Grupo DRE: "Proveitos e Ganhos" | "Custos e Perdas" | "" (não afecta DRE)
    /// </summary>
    public string GrupoDre         { get; set; } = string.Empty;

    /// <summary>
    /// Grupo Balanço: "AtivoCorrente" | "AtivoNaoCorrente" | "PassivoCorrente" |
    ///                "PassivoNaoCorrente" | "CapitalProprio" | "" (não afecta Balanço)
    /// </summary>
    public string GrupoBalanco     { get; set; } = string.Empty;

    /// <summary>
    /// Grupo Fluxo de Caixa: "Operacional" | "Investimento" | "Financiamento"
    /// </summary>
    public string GrupoFluxoCaixa  { get; set; } = string.Empty;

    /// <summary>
    /// Quando true, o motor deve calcular e lançar Imposto de Selo (1 %)
    /// nas contas 65 / 34 adicionalmente ao lançamento principal.
    /// </summary>
    public bool   AplicaImpostoSelo { get; set; } = false;

    public bool   Ativo             { get; set; } = true;
}
