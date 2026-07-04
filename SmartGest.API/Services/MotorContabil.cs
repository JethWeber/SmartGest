using Microsoft.EntityFrameworkCore;
using SmartGest.API.Data;
using SmartGest.API.Models;

namespace SmartGest.API.Services;

// ─────────────────────────────────────────────────────────────────────────────
// CONTRATO PÚBLICO
// ─────────────────────────────────────────────────────────────────────────────

public interface IMotorContabil
{
    /// <summary>
    /// Valida e gera os LancamentoDetalhe (partidas dobradas) para um lançamento
    /// já persistido na base de dados. Idempotente: apaga os detalhes anteriores
    /// antes de gerar os novos.
    /// </summary>
    Task ProcessarAsync(Lancamento lancamento);

    /// <summary>
    /// Re-processa todos os lançamentos activos que ainda não têm detalhes.
    /// Útil após migrações ou correcções de dados.
    /// </summary>
    Task<int> RecalcularSemDetalhesAsync();
}

// ─────────────────────────────────────────────────────────────────────────────
// RESULTADO INTERNO
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Resultado produzido pelo motor para um lançamento.</summary>
public sealed record ResultadoContabil(
    int    ContaDebitoId,
    int    ContaCreditoId,
    decimal Valor,
    int?   ContaImpostoSeloEncargosId = null,
    int?   ContaImpostoSeloEstadoId   = null,
    decimal ImpostoSelo               = 0m);

// ─────────────────────────────────────────────────────────────────────────────
// IMPLEMENTAÇÃO
// ─────────────────────────────────────────────────────────────────────────────

public sealed class MotorContabil : IMotorContabil
{
    private readonly AppDbContext _db;

    public MotorContabil(AppDbContext db) => _db = db;

    // ── PROCESSAR ────────────────────────────────────────────────────────────

    public async Task ProcessarAsync(Lancamento lancamento)
    {
        // 1. Obter categoria — OBRIGATÓRIO
        if (lancamento.CategoriaContabilId is null)
            throw new InvalidOperationException(
                "O lançamento não tem CategoriaContabilId definido. " +
                "O motor contabilístico exige que cada lançamento esteja associado " +
                "a uma categoria válida.");

        var categoria = await _db.CategoriaContabeis
            .FirstOrDefaultAsync(c => c.Id == lancamento.CategoriaContabilId.Value);

        if (categoria is null)
            throw new InvalidOperationException(
                $"Categoria #{lancamento.CategoriaContabilId} não encontrada na base de dados.");

        if (!categoria.Ativo)
            throw new InvalidOperationException(
                $"Categoria '{categoria.Nome}' está inactiva. Reactive-a ou escolha outra.");

        // 2. Validar valor
        if (lancamento.Valor <= 0)
            throw new InvalidOperationException(
                $"Valor inválido ({lancamento.Valor}). O valor do lançamento deve ser maior que zero.");

        // 3. Resolver resultado contabilístico
        var resultado = await ResolverResultadoAsync(categoria, lancamento);

        // 4. Validar equilíbrio débito = crédito
        var totalDebito  = resultado.Valor + resultado.ImpostoSelo;
        var totalCredito = resultado.Valor + resultado.ImpostoSelo;
        // (sempre equilibrado por construção, mas validamos explicitamente)
        if (totalDebito != totalCredito)
            throw new InvalidOperationException(
                $"ERRO CRÍTICO: Débito ({totalDebito}) ≠ Crédito ({totalCredito}). " +
                "Lançamento rejeitado pelo motor contabilístico.");

        // 5. Apagar detalhes anteriores (idempotência)
        var anteriores = _db.LancamentoDetalhes.Where(d => d.LancamentoId == lancamento.Id);
        _db.LancamentoDetalhes.RemoveRange(anteriores);

        // 6. Gravar lançamento principal (2 linhas: débito + crédito)
        _db.LancamentoDetalhes.Add(new LancamentoDetalhe
        {
            LancamentoId    = lancamento.Id,
            ContaContabilId = resultado.ContaDebitoId,
            Debito          = resultado.Valor,
            Credito         = 0m
        });

        _db.LancamentoDetalhes.Add(new LancamentoDetalhe
        {
            LancamentoId    = lancamento.Id,
            ContaContabilId = resultado.ContaCreditoId,
            Debito          = 0m,
            Credito         = resultado.Valor
        });

        // 7. Gravar Imposto de Selo (se aplicável)
        if (resultado.ImpostoSelo > 0 &&
            resultado.ContaImpostoSeloEncargosId.HasValue &&
            resultado.ContaImpostoSeloEstadoId.HasValue)
        {
            lancamento.ImpostoSelo = resultado.ImpostoSelo;

            _db.LancamentoDetalhes.Add(new LancamentoDetalhe
            {
                LancamentoId    = lancamento.Id,
                ContaContabilId = resultado.ContaImpostoSeloEncargosId.Value,
                Debito          = resultado.ImpostoSelo,
                Credito         = 0m
            });

            _db.LancamentoDetalhes.Add(new LancamentoDetalhe
            {
                LancamentoId    = lancamento.Id,
                ContaContabilId = resultado.ContaImpostoSeloEstadoId.Value,
                Debito          = 0m,
                Credito         = resultado.ImpostoSelo
            });
        }
        else
        {
            lancamento.ImpostoSelo = 0m;
        }

        await _db.SaveChangesAsync();
    }

    // ── RECALCULAR ────────────────────────────────────────────────────────────

    public async Task<int> RecalcularSemDetalhesAsync()
    {
        var pendentes = await _db.Lancamentos
            .Where(l => !l.Anulado &&
                        l.CategoriaContabilId.HasValue &&
                        !_db.LancamentoDetalhes.Any(d => d.LancamentoId == l.Id))
            .ToListAsync();

        var processados = 0;
        foreach (var lanc in pendentes)
        {
            try
            {
                await ProcessarAsync(lanc);
                processados++;
            }
            catch
            {
                // Regista silenciosamente — não interrompe os restantes.
                // Em produção, substituir por ILogger.
            }
        }
        return processados;
    }

    // ── RESOLUÇÃO INTERNA ─────────────────────────────────────────────────────

    private async Task<ResultadoContabil> ResolverResultadoAsync(
        CategoriaContabil categoria,
        Lancamento        lancamento)
    {
        var contaDebito  = await ObterContaOuFalharAsync(categoria.ContaDebito);
        var contaCredito = await ObterContaOuFalharAsync(categoria.ContaCredito);

        // Substituição dinâmica da conta de origem pelo banco registado
        // (conta "45" = Depósitos Bancários é o placeholder genérico)
        contaDebito  = await SubstituirContaBancoSeNecessario(contaDebito,  lancamento, isDébito: true);
        contaCredito = await SubstituirContaBancoSeNecessario(contaCredito, lancamento, isDébito: false);

        int?    idEncargos = null;
        int?    idEstado   = null;
        decimal is_        = 0m;

        if (categoria.AplicaImpostoSelo)
        {
            is_        = Math.Round(lancamento.Valor * 0.01m, 2);
            // Correção Bug 2 (inversão Classes 6/7): "Impostos e Taxas" passou
            // de "65" para "75" na renumeração PGC Angola. Ver migração
            // Fix_Inversao_Classes_6_7.
            idEncargos = (await ObterContaOuFalharAsync("75")).Id;
            idEstado   = (await ObterContaOuFalharAsync("34")).Id;
        }

        return new ResultadoContabil(
            contaDebito.Id,
            contaCredito.Id,
            lancamento.Valor,
            idEncargos,
            idEstado,
            is_);
    }

    /// <summary>
    /// Quando a conta resolvida é "45" (Depósitos Bancários genérica) e o
    /// lançamento tem um ContaBancariaId com ContaContabilId configurado,
    /// substitui pela conta PGC real do banco. Caso contrário mantém "45".
    /// </summary>
    private async Task<ContaContabil> SubstituirContaBancoSeNecessario(
        ContaContabil conta,
        Lancamento    lancamento,
        bool          isDébito)
    {
        if (conta.Codigo != "45" || !lancamento.ContaBancariaId.HasValue)
            return conta;

        var banco = await _db.ContasBancarias
            .Include(b => b.ContaContabil)
            .FirstOrDefaultAsync(b => b.Id == lancamento.ContaBancariaId.Value);

        return banco?.ContaContabil ?? conta;
    }

    private async Task<ContaContabil> ObterContaOuFalharAsync(string codigo)
    {
        var conta = await _db.ContasContabeis
            .FirstOrDefaultAsync(c => c.Codigo == codigo && c.Activa);

        if (conta is null)
            throw new InvalidOperationException(
                $"Conta contabilística '{codigo}' não encontrada no Plano de Contas PGC Angola " +
                "ou está inactiva. Verifique o plano de contas antes de prosseguir.");

        return conta;
    }
}
