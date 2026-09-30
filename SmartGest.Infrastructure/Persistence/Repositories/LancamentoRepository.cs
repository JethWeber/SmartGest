using SmartGest.Application.Abstractions;
using SmartGest.Core.Domain;

namespace SmartGest.Infrastructure.Persistence.Repositories;

public sealed class LancamentoRepository(SmartGestDbContext db) : ILancamentoRepository
{
    public Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        db.Lancamentos.Add(lancamento);
        return Task.CompletedTask;
    }
}
