using SmartGest.API.Models;

namespace SmartGest.API.Data;

/// <summary>
/// Seed das 40 categorias contabilísticas oficiais do SmartGest V2.
/// Mapeamento baseado no PGC Angola (Decreto n.º 82/01).
///
/// O utilizador vê apenas o Nome.
/// O MotorContabil usa ContaDebito e ContaCredito para gerar as partidas dobradas.
/// </summary>
public static class CategoriaContabilSeed
{
    public static IEnumerable<CategoriaContabil> Obter() =>
    [
        // ── 20 CATEGORIAS DE ENTRADA ──────────────────────────────────────────
        //  Id  Nome                          Tipo       Débito  Crédito  GrupoDRE                     GrupoBalanco     GrupoFluxo       IS
        new() { Id =  1, Nome = "Venda de Mercadorias",       Tipo = "Entrada", ContaDebito = "45", ContaCredito = "71", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = true,  Ativo = true },
        new() { Id =  2, Nome = "Prestação de Serviços",      Tipo = "Entrada", ContaDebito = "45", ContaCredito = "72", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = true,  Ativo = true },
        new() { Id =  3, Nome = "Recebimento de Cliente",     Tipo = "Entrada", ContaDebito = "45", ContaCredito = "31", GrupoDre = "",                   GrupoBalanco = "AtivoCorrente",  GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id =  4, Nome = "Adiantamento de Cliente",    Tipo = "Entrada", ContaDebito = "45", ContaCredito = "31", GrupoDre = "",                   GrupoBalanco = "PassivoCorrente",GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id =  5, Nome = "Juros Recebidos",            Tipo = "Entrada", ContaDebito = "45", ContaCredito = "78", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Financiamento",  AplicaImpostoSelo = false, Ativo = true },
        new() { Id =  6, Nome = "Outros Rendimentos",         Tipo = "Entrada", ContaDebito = "45", ContaCredito = "73", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id =  7, Nome = "Capital Inicial",            Tipo = "Entrada", ContaDebito = "45", ContaCredito = "51", GrupoDre = "",                   GrupoBalanco = "CapitalProprio", GrupoFluxoCaixa = "Financiamento",  AplicaImpostoSelo = false, Ativo = true },
        new() { Id =  8, Nome = "Aporte de Sócio",            Tipo = "Entrada", ContaDebito = "45", ContaCredito = "51", GrupoDre = "",                   GrupoBalanco = "CapitalProprio", GrupoFluxoCaixa = "Financiamento",  AplicaImpostoSelo = false, Ativo = true },
        new() { Id =  9, Nome = "Empréstimo Bancário",        Tipo = "Entrada", ContaDebito = "45", ContaCredito = "33", GrupoDre = "",                   GrupoBalanco = "PassivoNaoCorrente", GrupoFluxoCaixa = "Financiamento", AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 10, Nome = "Recebimento de Crédito",     Tipo = "Entrada", ContaDebito = "45", ContaCredito = "31", GrupoDre = "",                   GrupoBalanco = "AtivoCorrente",  GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 11, Nome = "Reembolso de Imposto",       Tipo = "Entrada", ContaDebito = "45", ContaCredito = "34", GrupoDre = "",                   GrupoBalanco = "AtivoCorrente",  GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 12, Nome = "Venda de Ativo",             Tipo = "Entrada", ContaDebito = "45", ContaCredito = "79", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Investimento",   AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 13, Nome = "Ganho Cambial",              Tipo = "Entrada", ContaDebito = "45", ContaCredito = "78", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Financiamento",  AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 14, Nome = "Dividendos Recebidos",       Tipo = "Entrada", ContaDebito = "45", ContaCredito = "78", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Financiamento",  AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 15, Nome = "Doações Recebidas",          Tipo = "Entrada", ContaDebito = "45", ContaCredito = "73", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 16, Nome = "Recuperação de Dívida",      Tipo = "Entrada", ContaDebito = "45", ContaCredito = "31", GrupoDre = "",                   GrupoBalanco = "AtivoCorrente",  GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 17, Nome = "Transferência Recebida",     Tipo = "Entrada", ContaDebito = "45", ContaCredito = "45", GrupoDre = "",                   GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 18, Nome = "Receita Extraordinária",     Tipo = "Entrada", ContaDebito = "45", ContaCredito = "79", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 19, Nome = "Comissão Recebida",          Tipo = "Entrada", ContaDebito = "45", ContaCredito = "72", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 20, Nome = "Outras Entradas",            Tipo = "Entrada", ContaDebito = "45", ContaCredito = "73", GrupoDre = "Proveitos e Ganhos", GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },

        // ── 20 CATEGORIAS DE SAÍDA ────────────────────────────────────────────
        new() { Id = 21, Nome = "Compra de Mercadorias",      Tipo = "Saída",   ContaDebito = "61", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "AtivoCorrente",  GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 22, Nome = "Pagamento a Fornecedor",     Tipo = "Saída",   ContaDebito = "32", ContaCredito = "45", GrupoDre = "",                   GrupoBalanco = "PassivoCorrente",GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 23, Nome = "Salários",                   Tipo = "Saída",   ContaDebito = "63", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 24, Nome = "INSS",                       Tipo = "Saída",   ContaDebito = "63", ContaCredito = "36", GrupoDre = "Custos e Perdas",    GrupoBalanco = "PassivoCorrente",GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 25, Nome = "IRT",                        Tipo = "Saída",   ContaDebito = "65", ContaCredito = "34", GrupoDre = "Custos e Perdas",    GrupoBalanco = "PassivoCorrente",GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 26, Nome = "IVA",                        Tipo = "Saída",   ContaDebito = "65", ContaCredito = "34", GrupoDre = "Custos e Perdas",    GrupoBalanco = "PassivoCorrente",GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 27, Nome = "Energia",                    Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 28, Nome = "Água",                       Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 29, Nome = "Internet",                   Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 30, Nome = "Telefone",                   Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 31, Nome = "Aluguer",                    Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 32, Nome = "Combustível",                Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 33, Nome = "Transportes",                Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 34, Nome = "Publicidade",                Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 35, Nome = "Consultoria",                Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 36, Nome = "Compra de Equipamento",      Tipo = "Saída",   ContaDebito = "11", ContaCredito = "45", GrupoDre = "",                   GrupoBalanco = "AtivoNaoCorrente",GrupoFluxoCaixa = "Investimento",  AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 37, Nome = "Manutenção",                 Tipo = "Saída",   ContaDebito = "62", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 38, Nome = "Juros Bancários",            Tipo = "Saída",   ContaDebito = "68", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Financiamento",  AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 39, Nome = "Amortização de Empréstimo",  Tipo = "Saída",   ContaDebito = "33", ContaCredito = "45", GrupoDre = "",                   GrupoBalanco = "PassivoNaoCorrente",GrupoFluxoCaixa = "Financiamento", AplicaImpostoSelo = false, Ativo = true },
        new() { Id = 40, Nome = "Outras Despesas",            Tipo = "Saída",   ContaDebito = "66", ContaCredito = "45", GrupoDre = "Custos e Perdas",    GrupoBalanco = "",               GrupoFluxoCaixa = "Operacional",    AplicaImpostoSelo = false, Ativo = true },
    ];
}
