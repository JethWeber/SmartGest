using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.Models;

namespace SmartGest.API.Services;

public record ParContabil(string CodigoDebito, string CodigoCredito);

public static class MapeamentoContabil
{
    private static readonly Dictionary<(string categoria, string tipo), ParContabil> _mapa = new()
    {
        // ── VENDAS E RECEITAS ──────────────────────────────────────────────
        { ("venda de mercadoria",              "Entrada"), new("45", "71") },
        { ("venda de mercadorias",             "Entrada"), new("45", "71") },
        { ("venda de produto",                 "Entrada"), new("45", "71") },
        { ("venda de produtos",                "Entrada"), new("45", "71") },
        { ("prestação de serviço",             "Entrada"), new("45", "72") },
        { ("prestação de serviços",            "Entrada"), new("45", "72") },
        { ("serviços prestados",               "Entrada"), new("45", "72") },
        { ("recebimento de cliente",           "Entrada"), new("45", "31") },
        { ("recebimento de clientes",          "Entrada"), new("45", "31") },
        { ("outros rendimentos",               "Entrada"), new("45", "73") },
        { ("outros rendimentos operacionais",  "Entrada"), new("45", "73") },
        { ("juros recebidos",                  "Entrada"), new("45", "78") },
        { ("ganhos financeiros",               "Entrada"), new("45", "78") },
        { ("receita financeira",               "Entrada"), new("45", "78") },
        { ("receitas financeiras",             "Entrada"), new("45", "78") },
        { ("outras receitas",                  "Entrada"), new("45", "73") },
        { ("receita diversa",                  "Entrada"), new("45", "79") },
        { ("receitas diversas",                "Entrada"), new("45", "79") },
        { ("receita extraordinária",           "Entrada"), new("45", "79") },
        { ("adiantamento de cliente",          "Entrada"), new("45", "31") },
        { ("entrada em caixa",                 "Entrada"), new("43", "45") },
        { ("capital integralizado",            "Entrada"), new("45", "51") },
        { ("aporte de capital",                "Entrada"), new("45", "51") },
        { ("empréstimo recebido",              "Entrada"), new("45", "33") },
        { ("empréstimo bancário",              "Entrada"), new("45", "33") },
        { ("transferência entre contas",       "Entrada"), new("45", "45") },
        { ("levantamento",                     "Entrada"), new("43", "45") },

        // ── DESPESAS ADMINISTRATIVAS ───────────────────────────────────────
        { ("despesa administrativa",           "Saída"),   new("62", "45") },
        { ("despesas administrativas",         "Saída"),   new("62", "45") },
        { ("material de escritório",           "Saída"),   new("62", "45") },
        { ("material de escritorio",           "Saída"),   new("62", "45") },
        { ("aluguer",                          "Saída"),   new("62", "45") },
        { ("arrendamento",                     "Saída"),   new("62", "45") },
        { ("água",                             "Saída"),   new("62", "45") },
        { ("energia eléctrica",                "Saída"),   new("62", "45") },
        { ("electricidade",                    "Saída"),   new("62", "45") },
        { ("internet",                         "Saída"),   new("62", "45") },
        { ("telefone",                         "Saída"),   new("62", "45") },
        { ("comunicações",                     "Saída"),   new("62", "45") },
        { ("combustível",                      "Saída"),   new("62", "45") },
        { ("combustivel",                      "Saída"),   new("62", "45") },
        { ("transportes",                      "Saída"),   new("62", "45") },
        { ("seguros",                          "Saída"),   new("62", "45") },
        { ("publicidade",                      "Saída"),   new("62", "45") },
        { ("marketing",                        "Saída"),   new("62", "45") },
        { ("consultoria",                      "Saída"),   new("62", "45") },
        { ("manutenção",                       "Saída"),   new("62", "45") },
        { ("manutencao",                       "Saída"),   new("62", "45") },
        { ("limpeza",                          "Saída"),   new("62", "45") },
        { ("segurança",                        "Saída"),   new("62", "45") },

        // ── SALÁRIOS E PESSOAL ─────────────────────────────────────────────
        { ("salário",                          "Saída"),   new("63", "45") },
        { ("salários",                         "Saída"),   new("63", "45") },
        { ("salario",                          "Saída"),   new("63", "45") },
        { ("salarios",                         "Saída"),   new("63", "45") },
        { ("vencimentos",                      "Saída"),   new("63", "45") },
        { ("remuneração",                      "Saída"),   new("63", "45") },
        { ("remuneracoes",                     "Saída"),   new("63", "45") },
        { ("pagamento de salário",             "Saída"),   new("63", "45") },
        { ("pagamento de salários",            "Saída"),   new("63", "45") },
        { ("pagamento de pessoal",             "Saída"),   new("63", "45") },
        { ("subsídio",                         "Saída"),   new("63", "45") },
        { ("subsidio",                         "Saída"),   new("63", "45") },
        { ("férias",                           "Saída"),   new("63", "45") },
        { ("ferias",                           "Saída"),   new("63", "45") },
        { ("décimo terceiro",                  "Saída"),   new("63", "45") },
        { ("encargos sociais",                 "Saída"),   new("63", "45") },
        { ("segurança social",                 "Saída"),   new("63", "36") },
        { ("inss",                             "Saída"),   new("63", "36") },

        // ── FORNECEDORES ───────────────────────────────────────────────────
        { ("pagamento a fornecedor",           "Saída"),   new("32", "45") },
        { ("pagamento a fornecedores",         "Saída"),   new("32", "45") },
        { ("fornecedor",                       "Saída"),   new("32", "45") },
        { ("fornecedores",                     "Saída"),   new("32", "45") },
        { ("compra de mercadoria",             "Saída"),   new("61", "45") },
        { ("compra de mercadorias",            "Saída"),   new("61", "45") },
        { ("compra de produto",                "Saída"),   new("61", "45") },
        { ("compra de produtos",               "Saída"),   new("61", "45") },
        { ("custo das mercadorias",            "Saída"),   new("61", "45") },
        { ("cmv",                              "Saída"),   new("61", "45") },

        // ── IMPOSTOS ───────────────────────────────────────────────────────
        { ("imposto",                          "Saída"),   new("65", "45") },
        { ("impostos",                         "Saída"),   new("65", "45") },
        { ("irt",                              "Saída"),   new("65", "34") },
        { ("iva",                              "Saída"),   new("65", "34") },
        { ("imposto sobre rendimento",         "Saída"),   new("65", "34") },
        { ("imposto sobre rendimentos",        "Saída"),   new("65", "34") },
        { ("taxa",                             "Saída"),   new("65", "45") },
        { ("taxas",                            "Saída"),   new("65", "45") },

        // ── ENCARGOS BANCÁRIOS ─────────────────────────────────────────────
        { ("encargo bancário",                 "Saída"),   new("68", "45") },
        { ("encargos bancários",               "Saída"),   new("68", "45") },
        { ("encargos bancarios",               "Saída"),   new("68", "45") },
        { ("juros pagos",                      "Saída"),   new("68", "45") },
        { ("comissão bancária",                "Saída"),   new("68", "45") },
        { ("comissoes bancarias",              "Saída"),   new("68", "45") },
        { ("taxa de juro",                     "Saída"),   new("68", "45") },
        { ("juro",                             "Saída"),   new("68", "45") },
        { ("juros",                            "Saída"),   new("68", "45") },

        // ── INVESTIMENTOS / ACTIVOS FIXOS ─────────────────────────────────
        { ("compra de equipamento",            "Saída"),   new("11", "45") },
        { ("compra de activo",                 "Saída"),   new("11", "45") },
        { ("activo fixo",                      "Saída"),   new("11", "45") },
        { ("imobilizado",                      "Saída"),   new("11", "45") },
        { ("depreciação",                      "Saída"),   new("64", "18") },
        { ("depreciacao",                      "Saída"),   new("64", "18") },
        { ("amortização",                      "Saída"),   new("64", "18") },
        { ("amortizacao",                      "Saída"),   new("64", "18") },

        // ── EMPRÉSTIMOS ────────────────────────────────────────────────────
        { ("amortização de empréstimo",        "Saída"),   new("33", "45") },
        { ("amortizacao de emprestimo",        "Saída"),   new("33", "45") },
        { ("reembolso de empréstimo",          "Saída"),   new("33", "45") },
        { ("pagamento de empréstimo",          "Saída"),   new("33", "45") },

        // ── OUTROS ────────────────────────────────────────────────────────
        { ("outros custos",                    "Saída"),   new("66", "45") },
        { ("outros custos operacionais",       "Saída"),   new("66", "45") },
        { ("custo extraordinário",             "Saída"),   new("69", "45") },
        { ("custos extraordinários",           "Saída"),   new("69", "45") },
        { ("outros",                           "Saída"),   new("66", "45") },
        { ("depósito bancário",                "Saída"),   new("45", "43") },
    };

    private static readonly Dictionary<string, ParContabil> _fallback = new()
    {
        { "Entrada", new("45", "73") },
        { "Saída",   new("66", "45") },
    };

    public static ParContabil Resolver(string categoria, string tipo)
    {
        var chave = (categoria.Trim().ToLowerInvariant(), tipo);
        if (_mapa.TryGetValue(chave, out var par)) return par;

        var catLower = categoria.Trim().ToLowerInvariant();
        foreach (var kv in _mapa)
        {
            if (kv.Key.tipo == tipo && catLower.Contains(kv.Key.categoria))
                return kv.Value;
        }

        return _fallback[tipo == "Entrada" ? "Entrada" : "Saída"];
    }

    /// <summary>
    /// Devolve as categorias disponíveis para a UI, filtradas por tipo.
    /// Cada categoria aparece apenas uma vez (sem duplicados).
    /// </summary>
    public static List<CategoriaDto> ListarCategorias(string? tipo = null)
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("pt-PT");

        return _mapa.Keys
            .Where(k => tipo == null || k.tipo == tipo)
            .Select(k => new CategoriaDto(
                culture.TextInfo.ToTitleCase(k.categoria),
                k.tipo))
            .DistinctBy(c => c.Nome.ToLowerInvariant())
            .OrderBy(c => c.Nome)
            .ToList();
    }
}

public record CategoriaDto(string Nome, string Tipo);

// ─────────────────────────────────────────────────────────────────────────────
// SERVIÇO CONTABILÍSTICO
// ─────────────────────────────────────────────────────────────────────────────

public class ContabilidadeService
{
    private readonly AppDbContext _db;
    public ContabilidadeService(AppDbContext db) => _db = db;

    public async Task GerarDetalhesAsync(Lancamento lancamento)
    {
        var existentes = await _db.LancamentoDetalhes
            .Where(d => d.LancamentoId == lancamento.Id)
            .ToListAsync();

        if (existentes.Count > 0)
            _db.LancamentoDetalhes.RemoveRange(existentes);

        ParContabil par;
        if (lancamento.CategoriaContabilId.HasValue)
        {
            var cat = await _db.Set<CategoriaContabil>().FindAsync(lancamento.CategoriaContabilId.Value);
            if (cat is null)
                throw new InvalidOperationException("Categoria contabilística não encontrada.");
            par = new ParContabil(cat.ContaDebito, cat.ContaCredito);
        }
        else
        {
            par = MapeamentoContabil.Resolver(lancamento.Categoria, lancamento.Tipo);
        }

        var contaDebito  = await ObterContaOuFalharAsync(par.CodigoDebito);
        var contaCredito = await ObterContaOuFalharAsync(par.CodigoCredito);
        var valor        = lancamento.Valor;

        _db.LancamentoDetalhes.Add(new LancamentoDetalhe
        {
            LancamentoId    = lancamento.Id,
            ContaContabilId = contaDebito.Id,
            Debito          = valor,
            Credito         = 0
        });

        _db.LancamentoDetalhes.Add(new LancamentoDetalhe
        {
            LancamentoId    = lancamento.Id,
            ContaContabilId = contaCredito.Id,
            Debito          = 0,
            Credito         = valor
        });

        await _db.SaveChangesAsync();
    }

    public async Task<int> RecalcularLancamentosSemDetalhesAsync()
    {
        var lancamentosSemDetalhes = await _db.Lancamentos
            .Where(l => !l.Anulado && !_db.LancamentoDetalhes.Any(d => d.LancamentoId == l.Id))
            .ToListAsync();

        var processados = 0;
        foreach (var lanc in lancamentosSemDetalhes)
        {
            try   { await GerarDetalhesAsync(lanc); processados++; }
            catch { /* ignora categorias sem mapeamento */ }
        }

        return processados;
    }

    private async Task<ContaContabil> ObterContaOuFalharAsync(string codigo)
    {
        var conta = await _db.ContasContabeis
            .FirstOrDefaultAsync(c => c.Codigo == codigo && c.Activa);

        if (conta is null)
            throw new InvalidOperationException(
                $"Conta contabilística '{codigo}' não encontrada ou inactiva.");

        return conta;
    }
}