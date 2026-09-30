using SmartGest.Application.Abstractions;

namespace SmartGest.Infrastructure.Persistence;

public sealed class UnitOfWork(SmartGestDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
