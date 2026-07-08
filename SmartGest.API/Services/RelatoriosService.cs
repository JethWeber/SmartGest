using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.DTOs.Responses;
using SmartGest.API.Models;

namespace SmartGest.API.Services;

/// <summary>
/// Serviço de relatórios V2.
///
/// PRINCÍPIO: todos os relatórios consomem LancamentoDetalhe (partidas dobradas)
/// gerados pelo MotorContabil. Nenhum relatório calcula regras contabilísticas.
///
/// NOTA (correção Bug 2 — Classes 6/7): a classificação Receita/Despesa usa o
/// campo ContaContabil.Grupo ("Receita" / "Despesa"), não o prefixo do Codigo.
/// Isto evita reintroduzir a inversão de classes caso a numeração volte a mudar.
/// </summary>
public class RelatoriosService
{
    private readonly AppDbContext _db;
    public RelatoriosService(AppDbContext db) => _db = db;

    // ─────────────────────────────────────────────────────────────────────────
    // BALANCETE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<object> ObterBalanceteAsync(
        DateTime? dataInicio, DateTime? dataFim, string? grupo)
    {
        var inicio = (dataInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var fim    = (dataFim ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

        var detalhesAtivos = _db.LancamentoDetalhes
            .Where(d => !d.Lancamento!.Anulado);

        var movPeriodo = await detalhesAtivos
            .Where(d => d.Lancamento!.Data >= inicio && d.Lancamento.Data <= fim)
            .GroupBy(d => d.ContaContabilId)
            .Select(g => new { ContaContabilId = g.Key,
                               TotalDebito  = g.Sum(d => d.Debito),
                               TotalCredito = g.Sum(d => d.Credito) })
            .ToListAsync();

        var movAnterior = await detalhesAtivos
            .Where(d => d.Lancamento!.Data < inicio)
            .GroupBy(d => d.ContaContabilId)
            .Select(g => new { ContaContabilId = g.Key,
                               TotalDebito  = g.Sum(d => d.Debito),
                               TotalCredito = g.Sum(d => d.Credito) })
            .ToListAsync();

        var contas = await _db.ContasContabeis
            .Where(c => c.Activa && (grupo == null || c.Grupo == grupo))
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        var periodoDict  = movPeriodo.ToDictionary(x => x.ContaContabilId);
        var anteriorDict = movAnterior.ToDictionary(x => x.ContaContabilId);

        var items = contas.Select(conta =>
        {
            periodoDict.TryGetValue(conta.Id, out var mov);
            anteriorDict.TryGetValue(conta.Id, out var ant);

            var movDeb = mov?.TotalDebito  ?? 0m;
            var movCre = mov?.TotalCredito ?? 0m;
            var antDeb = ant?.TotalDebito  ?? 0m;
            var antCre = ant?.TotalCredito ?? 0m;

            var saldoLiqAnterior = antDeb - antCre;
            var saldoAntDeb = conta.IsDevedora
                ? Math.Max(0,  saldoLiqAnterior)
                : Math.Max(0, -saldoLiqAnterior);
            var saldoAntCre = conta.IsDevedora
                ? Math.Max(0, -saldoLiqAnterior)
                : Math.Max(0,  saldoLiqAnterior);

            var saldoFinalLiq = saldoLiqAnterior + movDeb - movCre;
            var saldoFinalDeb = Math.Max(0,  saldoFinalLiq);
            var saldoFinalCre = Math.Max(0, -saldoFinalLiq);

            return new BalanceteItemResponse(
                conta.Codigo, conta.Nome, conta.Grupo,
                saldoAntDeb, saldoAntCre,
                movDeb, movCre,
                saldoFinalDeb, saldoFinalCre);
        }).ToList();

        var totalDebitos  = items.Sum(i => i.MovDebito  + i.SaldoAnteriorDebito);
        var totalCreditos = items.Sum(i => i.MovCredito + i.SaldoAnteriorCredito);

        // Validação de equilíbrio — log de auditoria se desequilibrado
        var equilibrado = totalDebitos == totalCreditos;

        return new
        {
            periodo      = new { inicio, fim },
            totalDebitos,
            totalCreditos,
            equilibrado,
            items
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DRE — Demonstração de Resultados do Exercício
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<DreSumarioResponse> ObterDreAsync(
        DateTime? dataInicio, DateTime? dataFim)
    {
        var inicio = (dataInicio ?? new DateTime(DateTime.Today.Year, 1, 1)).Date;
        var fim    = (dataFim ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

        // DRE consome apenas contas de Despesa e Receita (classificação por Grupo,
        // não pelo prefixo do Codigo — ver nota no topo do ficheiro)
        var linhasDb = await _db.LancamentoDetalhes
            .Where(d =>
                !d.Lancamento!.Anulado &&
                d.Lancamento.Data >= inicio &&
                d.Lancamento.Data <= fim &&
                (d.ContaContabil!.Grupo == "Despesa" ||
                 d.ContaContabil.Grupo == "Receita"))
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
            .ToListAsync();

        var linhas = linhasDb.Select(x =>
        {
            var isReceita = x.Grupo == "Receita";
            var realizado = isReceita
                ? x.TotalCredito - x.TotalDebito
                : x.TotalDebito  - x.TotalCredito;

            return new DreItemResponse(
                x.Codigo,
                x.Nome,
                isReceita ? "Proveitos e Ganhos" : "Custos e Perdas",
                0m,         // orçado — módulo futuro
                realizado,
                isReceita,
                fim);
        }).ToList();

        var totalRec = linhas.Where(l =>  l.IsReceita).Sum(l => l.ValorRealizado);
        var totalCus = linhas.Where(l => !l.IsReceita).Sum(l => l.ValorRealizado);

        // Fluxo mensal DRE
        var anoRef = inicio.Year;
        var detalhesMensais = await _db.LancamentoDetalhes
            .Where(d =>
                !d.Lancamento!.Anulado &&
                d.Lancamento.Data >= new DateTime(anoRef, 1, 1) &&
                d.Lancamento.Data <= fim &&
                (d.ContaContabil!.Grupo == "Despesa" ||
                 d.ContaContabil.Grupo == "Receita"))
            .Select(d => new
            {
                d.Lancamento!.Data.Month,
                d.ContaContabil!.Grupo,
                d.Debito,
                d.Credito
            })
            .ToListAsync();

        var nomesMes = new[] { "Jan","Fev","Mar","Abr","Mai","Jun",
                               "Jul","Ago","Set","Out","Nov","Dez" };
        var fluxoMensal = Enumerable.Range(1, 12).Select(m =>
        {
            var doMes = detalhesMensais.Where(x => x.Month == m);
            var rec   = doMes.Where(x => x.Grupo == "Receita").Sum(x => x.Credito - x.Debito);
            var cus   = doMes.Where(x => x.Grupo == "Despesa").Sum(x => x.Debito  - x.Credito);
            return new FluxoMensalItem(nomesMes[m - 1], rec, cus, rec - cus);
        }).ToList();

        return new DreSumarioResponse(totalRec, totalCus, totalRec - totalCus, linhas, fluxoMensal);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BALANÇO PATRIMONIAL
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<BalancoResponse> ObterBalancoAsync(int? ano, int? mes)
    {
        var hoje   = DateTime.Today;
        var anoRef = ano ?? hoje.Year;
        var mesRef = mes ?? hoje.Month;

        var ate = new DateTime(anoRef, mesRef, DateTime.DaysInMonth(anoRef, mesRef))
                    .AddDays(1).AddTicks(-1);

        var saldosDb = await _db.LancamentoDetalhes
            .Where(d => !d.Lancamento!.Anulado && d.Lancamento.Data <= ate)
            .GroupBy(d => d.ContaContabilId)
            .Select(g => new
            {
                ContaContabilId = g.Key,
                TotalDebito  = g.Sum(d => d.Debito),
                TotalCredito = g.Sum(d => d.Credito)
            })
            .ToListAsync();

        var saldoPorConta = saldosDb.ToDictionary(x => x.ContaContabilId);
        var contas = await _db.ContasContabeis.Where(c => c.Activa).ToListAsync();

        decimal LiquidoConta(ContaContabil conta)
        {
            saldoPorConta.TryGetValue(conta.Id, out var s);
            var deb = s?.TotalDebito  ?? 0m;
            var cre = s?.TotalCredito ?? 0m;
            return conta.IsDevedora ? deb - cre : cre - deb;
        }

        // Math.Max preservado aqui de propósito — é o Bug #1 (Math.Max), tratado à parte
        decimal SaldoLinha(ContaContabil conta) => Math.Max(0, LiquidoConta(conta));

        List<BalancoLinhaResponse> LinhasPara(string grupo, bool? corrente) =>
            contas
                .Where(c => c.Grupo == grupo && c.Corrente == corrente)
                .OrderBy(c => c.Codigo)
                .Select(c => new BalancoLinhaResponse(
                    $"{c.Nome} ({c.Codigo})",
                    SaldoLinha(c),
                    c.Grupo == "Ativo" && !c.IsDevedora))   // dedução automática (ex.: Amortizações)
                .ToList();

        var ativoCorrentes      = LinhasPara("Ativo", true);
        var ativoNaoCorrentes   = LinhasPara("Ativo", false);
        var passivoCorrentes    = LinhasPara("Passivo", true);
        var passivoNaoCorrentes = LinhasPara("Passivo", false);

        decimal resultadoExercicio;
        var conta88 = contas.FirstOrDefault(c => c.Codigo == "88");
        if (conta88 != null && saldoPorConta.TryGetValue(conta88.Id, out var s88) && (s88.TotalCredito - s88.TotalDebito) != 0)
        {
            resultadoExercicio = s88.TotalCredito - s88.TotalDebito;
        }
        else
        {
            var totalProv = contas.Where(c => c.Grupo == "Receita").Sum(LiquidoConta);
            var totalCust = contas.Where(c => c.Grupo == "Despesa").Sum(LiquidoConta);
            resultadoExercicio = totalProv - totalCust;
        }

        var capitalProprio = LinhasPara("Capital", null);
        capitalProprio.Add(new BalancoLinhaResponse("Resultado do Exercício", resultadoExercicio));

        var totalAtivo = ativoCorrentes.Sum(x => x.Valor)
                    + ativoNaoCorrentes.Where(x => !x.IsDeducao).Sum(x => x.Valor)
                    - ativoNaoCorrentes.Where(x =>  x.IsDeducao).Sum(x => x.Valor);

        var totalPassivo = passivoCorrentes.Sum(x => x.Valor) + passivoNaoCorrentes.Sum(x => x.Valor);
        var totalCapital = capitalProprio.Sum(x => x.Valor);
        var totalPassivoMaisCapital = totalPassivo + totalCapital;

        return new BalancoResponse(
            ativoCorrentes, ativoNaoCorrentes,
            passivoCorrentes, passivoNaoCorrentes, capitalProprio,
            totalAtivo, totalPassivo, totalCapital, totalPassivoMaisCapital);
    }
    // ─────────────────────────────────────────────────────────────────────────
    // FLUXO DE CAIXA
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<FluxoCaixaResponse> ObterFluxoCaixaAsync(
        DateTime? dataInicio, DateTime? dataFim)
    {
        var inicio = (dataInicio ?? new DateTime(DateTime.Today.Year, 1, 1)).Date;
        var fim    = (dataFim ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

        // Saldo inicial = soma de tudo antes do período nas contas de caixa/banco (43/45)
        // Nota: 43/45 são Classe 4 (Meios Monetários) — não afetadas pela correção das Classes 6/7
        var saldoInicialDb = await _db.LancamentoDetalhes
            .Where(d =>
                !d.Lancamento!.Anulado &&
                d.Lancamento.Data < inicio &&
                (d.ContaContabil!.Codigo.StartsWith("43") ||
                 d.ContaContabil.Codigo.StartsWith("45")))
            .SumAsync(d => d.Debito - d.Credito);

        var lancamentosPeriodo = await _db.Lancamentos
            .Include(l => l.CategoriaContabil)
            .Where(l =>
                !l.Anulado &&
                l.Data >= inicio &&
                l.Data <= fim &&
                l.CategoriaContabilId.HasValue)
            .OrderBy(l => l.Data)
            .ToListAsync();

        var entradas = lancamentosPeriodo
            .Where(l => l.Tipo == "Entrada")
            .Select(l => new FluxoCaixaLinhaResponse(
                l.Descricao,
                l.Categoria,
                l.CategoriaContabil?.GrupoFluxoCaixa ?? "Operacional",
                l.Data,
                l.Valor,
                true))
            .ToList();

        var saidas = lancamentosPeriodo
            .Where(l => l.Tipo == "Saída")
            .Select(l => new FluxoCaixaLinhaResponse(
                l.Descricao,
                l.Categoria,
                l.CategoriaContabil?.GrupoFluxoCaixa ?? "Operacional",
                l.Data,
                l.Valor,
                false))
            .ToList();

        var totalEntradas = entradas.Sum(e => e.Valor);
        var totalSaidas   = saidas.Sum(s => s.Valor);
        var saldoFinal    = saldoInicialDb + totalEntradas - totalSaidas;

        return new FluxoCaixaResponse(
            saldoInicialDb,
            saldoFinal,
            totalEntradas,
            totalSaidas,
            entradas,
            saidas);
    }
}
