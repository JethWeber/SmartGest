using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.DTOs.Responses;

namespace SmartGest.API.Services;

public class RelatoriosService
{
    private readonly AppDbContext _db;

    public RelatoriosService(AppDbContext db) => _db = db;

    // ─────────────────────────────────────────────────────────────────────────
    // BALANCETE
    //
    // Performance: toda a aritmética de SUM é delegada ao PostgreSQL via LINQ
    // GroupBy/SumAsync. Nenhum registro histórico é carregado para a memória C#.
    //
    // Integridade: os saldos iniciais devedores/credores são calculados como
    // saldo líquido real, sem truncamento prematuro com Math.Max, para preservar
    // saldos transitórios inversamente naturais (ex: conta devedora com saldo
    // credor momentâneo).
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<object> ObterBalanceteAsync(DateTime? dataInicio, DateTime? dataFim, string? grupo)
    {
        var inicio = DateTime.SpecifyKind((dataInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date, DateTimeKind.Utc);
        var fim    = DateTime.SpecifyKind((dataFim    ?? DateTime.Today).Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // Base de detalhes ativos (exclui lançamentos anulados)
        var detalhesAtivos = _db.LancamentoDetalhes
            .Where(d => !d.Lancamento!.Anulado);

        // ── Agregação do período — executada no PostgreSQL ─────────────────
        var movimentosPeriodo = await detalhesAtivos
            .Where(d => d.Lancamento!.Data >= inicio && d.Lancamento.Data <= fim)
            .GroupBy(d => d.ContaContabilId)
            .Select(g => new
            {
                ContaContabilId = g.Key,
                TotalDebito  = g.Sum(d => d.Debito),
                TotalCredito = g.Sum(d => d.Credito)
            })
            .ToListAsync(); // resultado agregado — apenas N linhas (uma por conta)

        // ── Agregação anterior ao período — executada no PostgreSQL ────────
        var movimentosAnteriores = await detalhesAtivos
            .Where(d => d.Lancamento!.Data < inicio)
            .GroupBy(d => d.ContaContabilId)
            .Select(g => new
            {
                ContaContabilId = g.Key,
                TotalDebito  = g.Sum(d => d.Debito),
                TotalCredito = g.Sum(d => d.Credito)
            })
            .ToListAsync(); // resultado agregado — apenas N linhas (uma por conta)

        // ── Plano de contas ────────────────────────────────────────────────
        var contas = await _db.ContasContabeis
            .Where(c => c.Activa && (grupo == null || c.Grupo == grupo))
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        // Dicionários para lookup O(1) — sem iterações aninhadas
        var periodoDict   = movimentosPeriodo.ToDictionary(x => x.ContaContabilId);
        var anteriorDict  = movimentosAnteriores.ToDictionary(x => x.ContaContabilId);

        var items = contas.Select(conta =>
        {
            periodoDict.TryGetValue(conta.Id, out var mov);
            anteriorDict.TryGetValue(conta.Id, out var ant);

            var movDeb = mov?.TotalDebito  ?? 0m;
            var movCre = mov?.TotalCredito ?? 0m;

            var antDeb = ant?.TotalDebito  ?? 0m;
            var antCre = ant?.TotalCredito ?? 0m;

            // Saldo líquido anterior (sem truncamento — preserva integridade contabilística)
            var saldoLiqAnterior = antDeb - antCre;

            // Apresentação no balancete: separação em coluna Devedor / Credor
            var saldoAntDeb = conta.IsDevedora
                ?  Math.Max(0,  saldoLiqAnterior)  // conta devedora → excesso devedor
                :  Math.Max(0, -saldoLiqAnterior);  // conta credora vista pelo lado credor
            var saldoAntCre = conta.IsDevedora
                ?  Math.Max(0, -saldoLiqAnterior)  // excesso credor numa conta normalmente devedora
                :  Math.Max(0,  saldoLiqAnterior);

            // Saldo final = saldo anterior + movimentos do período
            var saldoFinalLiq = saldoLiqAnterior + movDeb - movCre;
            var saldoFinalDeb = Math.Max(0,  saldoFinalLiq);
            var saldoFinalCre = Math.Max(0, -saldoFinalLiq);

            return new BalanceteItemResponse(
                conta.Codigo, conta.Nome, conta.Grupo,
                saldoAntDeb, saldoAntCre,
                movDeb, movCre,
                saldoFinalDeb, saldoFinalCre);
        }).ToList();

        return new
        {
            periodo       = new { inicio, fim },
            totalDebitos  = items.Sum(i => i.MovDebito  + i.SaldoAnteriorDebito),
            totalCreditos = items.Sum(i => i.MovCredito + i.SaldoAnteriorCredito),
            items
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DRE — Demonstração de Resultados do Exercício
    //
    // Correção crítica: filtro por CÓDIGO NUMÉRICO de classe PGC Angola,
    // não por string "Receita"/"Despesa" do campo Grupo (que pode ser
    // configurado manualmente e causar resultados vazios).
    //
    //   Classe 6 (Código começa por "6") → Custos e Perdas
    //   Classe 7 (Código começa por "7") → Proveitos e Ganhos
    //
    // Toda a agregação é delegada ao PostgreSQL.
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<DreSumarioResponse> ObterDreAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        var inicio = DateTime.SpecifyKind((dataInicio ?? new DateTime(DateTime.Today.Year, 1, 1)).Date, DateTimeKind.Utc);
        var fim    = DateTime.SpecifyKind((dataFim    ?? DateTime.Today).Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // Agrega diretamente no PostgreSQL por conta, filtrando pelas classes 6 e 7
        var linhasDb = await _db.LancamentoDetalhes
            .Where(d =>
                !d.Lancamento!.Anulado &&
                d.Lancamento.Data >= inicio &&
                d.Lancamento.Data <= fim &&
                (d.ContaContabil!.Codigo.StartsWith("6") || d.ContaContabil.Codigo.StartsWith("7")))
            .GroupBy(d => new
            {
                d.ContaContabil!.Codigo,
                d.ContaContabil.Nome,
                d.ContaContabil.Grupo
            })
            .Select(g => new
            {
                g.Key.Codigo,
                g.Key.Nome,
                g.Key.Grupo,
                TotalDebito  = g.Sum(d => d.Debito),
                TotalCredito = g.Sum(d => d.Credito)
            })
            .OrderBy(x => x.Codigo)
            .ToListAsync(); // resultado compacto — uma linha por conta contabilística

        var linhas = linhasDb.Select(x =>
        {
            // Classe 7: proveitos — natureza credora → resultado = Crédito - Débito
            // Classe 6: custos   — natureza devedora → resultado = Débito  - Crédito
            var isReceita = x.Codigo.StartsWith("7");
            var realizado = isReceita
                ? x.TotalCredito - x.TotalDebito
                : x.TotalDebito  - x.TotalCredito;

            return new DreItemResponse(
                x.Codigo,
                x.Nome,
                isReceita ? "Proveitos e Ganhos" : "Custos e Perdas",
                realizado * 0.9m, // orçado (placeholder — substituir por módulo de orçamento real)
                realizado,
                isReceita,
                fim);
        }).ToList();

        var totalRec = linhas.Where(l =>  l.IsReceita).Sum(l => l.ValorRealizado);
        var totalCus = linhas.Where(l => !l.IsReceita).Sum(l => l.ValorRealizado);

        return new DreSumarioResponse(totalRec, totalCus, totalRec - totalCus, linhas);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BALANÇO PATRIMONIAL
    //
    // Mapeamento atualizado para os códigos PGC Angola:
    //   Caixa          → 43    Bancos         → 45
    //   Clientes       → 31    Fornecedores   → 32
    //   Inventários    → 22    Ativos Fixos   → 11
    //   Amort. Acum.   → 18 (retificativa — subtrai do Ativo)
    //   Capital Social → 51    Reservas       → 55
    //   Res. Transitados→ 59   Empréstimos    → 33
    //   Estado/IRT     → 34
    //   Resultado 88   → lido da conta 88 quando existir lançamento de encerramento;
    //                    caso contrário calculado dinamicamente classes 6 e 7.
    //
    // Performance: uma única query agrega todos os saldos no PostgreSQL,
    // retornando apenas o vetor de (ContaContabilId, SumDebito, SumCredito).
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<BalancoResponse> ObterBalancoAsync(int? ano, int? mes)
    {
        var hoje   = DateTime.Today;
        var anoRef = ano ?? hoje.Year;
        var mesRef = mes ?? hoje.Month;
        var ate    = DateTime.SpecifyKind(new DateTime(anoRef, mesRef, DateTime.DaysInMonth(anoRef, mesRef))
                         .AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // Uma única query agrega todos os movimentos até à data de referência
        // diretamente no PostgreSQL — zero dados históricos em memória RAM C#.
        var saldosDb = await _db.LancamentoDetalhes
            .Where(d => !d.Lancamento!.Anulado && d.Lancamento.Data <= ate)
            .GroupBy(d => d.ContaContabil!.Codigo)
            .Select(g => new
            {
                Codigo       = g.Key,
                TotalDebito  = g.Sum(d => d.Debito),
                TotalCredito = g.Sum(d => d.Credito)
            })
            .ToListAsync(); // compacto — uma linha por conta

        var saldoDict = saldosDb.ToDictionary(x => x.Codigo);

        // Retorna o saldo líquido de uma conta ou prefixo de código.
        // devedora=true  → saldo positivo é devedor  (Ativo)
        // devedora=false → saldo positivo é credor   (Passivo / Capital / Receita)
        decimal Saldo(string codigoOuPrefixo, bool devedora)
        {
            decimal totalDeb = 0m, totalCre = 0m;
            foreach (var kv in saldoDict)
            {
                if (kv.Key.StartsWith(codigoOuPrefixo))
                {
                    totalDeb += kv.Value.TotalDebito;
                    totalCre += kv.Value.TotalCredito;
                }
            }
            var liquido = devedora ? totalDeb - totalCre : totalCre - totalDeb;
            return Math.Max(0, liquido);
        }

        // ── ATIVO CORRENTE ─────────────────────────────────────────────────
        var ativoCorrentes = new List<BalancoLinhaResponse>
        {
            new("Caixa (43)",                          Saldo("43", true)),
            new("Depósitos Bancários (45)",            Saldo("45", true)),
            new("Clientes (31)",                       Saldo("31", true)),
            new("Mercadorias / Inventários (22)",      Saldo("22", true)),
            new("Matérias-Primas (26)",                Saldo("26", true)),
            new("Investimentos Financeiros CP (13)",   Saldo("13", true)),
        };

        // ── ATIVO NÃO CORRENTE ────────────────────────────────────────────
        var ativoFixoBruto  = Saldo("11", true);
        var amortAcumulada  = Saldo("18", false); // conta 18 é retificativa (credora)
        var ativosIntang    = Saldo("12", true);

        var ativoNaoCorrentes = new List<BalancoLinhaResponse>
        {
            new("Activos Fixos Tangíveis — bruto (11)", ativoFixoBruto),
            new("(–) Amortizações Acumuladas (18)",    -amortAcumulada, true),
            new("Activos Intangíveis (12)",             ativosIntang),
        };

        // ── PASSIVO CORRENTE ──────────────────────────────────────────────
        var passivoCorrentes = new List<BalancoLinhaResponse>
        {
            new("Fornecedores (32)",                   Saldo("32", false)),
            new("Estado e Entes Públicos — IRT/IVA (34)", Saldo("34", false)),
            new("Pessoal — Remunerações a Pagar (36)", Saldo("36", false)),
            new("Empréstimos Bancários CP (33)",       Saldo("33", false)),
        };

        // ── PASSIVO NÃO CORRENTE ──────────────────────────────────────────
        var passivoNaoCorrentes = new List<BalancoLinhaResponse>
        {
            new("Empréstimos Bancários LP (33)", 0), // diferenciação CP/LP requer sub-contas
            new("Provisões para Riscos e Encargos", 0),
        };

        // ── RESULTADO DO EXERCÍCIO ────────────────────────────────────────
        // Prioridade 1: ler o saldo da conta 88 (Resultado Líquido do Exercício),
        // caso o fecho de contas já tenha sido efetuado.
        // Prioridade 2: calcular dinamicamente pelas classes 6 e 7 se a conta 88
        // ainda não tem movimentos (exercício em curso).
        decimal resultadoExercicio;

        if (saldoDict.TryGetValue("88", out var conta88) &&
            (conta88.TotalCredito - conta88.TotalDebito) != 0)
        {
            // Conta 88 — natureza credora quando há lucro
            resultadoExercicio = conta88.TotalCredito - conta88.TotalDebito;
        }
        else
        {
            // Cálculo dinâmico: total de proveitos (7x) menos total de custos (6x)
            var totalProveitos = saldoDict
                .Where(kv => kv.Key.StartsWith("7"))
                .Sum(kv => kv.Value.TotalCredito - kv.Value.TotalDebito);

            var totalCustos = saldoDict
                .Where(kv => kv.Key.StartsWith("6"))
                .Sum(kv => kv.Value.TotalDebito - kv.Value.TotalCredito);

            resultadoExercicio = totalProveitos - totalCustos;
        }

        // ── CAPITAL PRÓPRIO ───────────────────────────────────────────────
        var capitalProprio = new List<BalancoLinhaResponse>
        {
            new("Capital Social (51)",          Saldo("51", false)),
            new("Reservas Legais (55)",         Saldo("55", false)),
            new("Resultados Transitados (59)",  Saldo("59", false)),
            new("Resultado do Exercício",       resultadoExercicio),
        };

        // ── TOTAIS ────────────────────────────────────────────────────────
        var totalAtivo              = ativoCorrentes.Sum(x => x.Valor)
                                    + ativoNaoCorrentes.Sum(x => x.Valor);
        var totalPassivo            = passivoCorrentes.Sum(x => x.Valor)
                                    + passivoNaoCorrentes.Sum(x => x.Valor);
        var totalCapital            = capitalProprio.Sum(x => x.Valor);
        var totalPassivoMaisCapital = totalPassivo + totalCapital;

        return new BalancoResponse(
            ativoCorrentes, ativoNaoCorrentes,
            passivoCorrentes, passivoNaoCorrentes, capitalProprio,
            totalAtivo, totalPassivo, totalCapital, totalPassivoMaisCapital);
    }
}
