using Microsoft.EntityFrameworkCore;
using SmartGest.Application.Abstractions;
using SmartGest.Core.Domain;

namespace SmartGest.Infrastructure.Persistence.Repositories;

public sealed class CategoriaContabilRepository(SmartGestDbContext db) : ICategoriaContabilRepository
{
    public Task<CategoriaContabil?> ObterAsync(int id, CancellationToken cancellationToken = default)
        => db.CategoriaContabeis.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
