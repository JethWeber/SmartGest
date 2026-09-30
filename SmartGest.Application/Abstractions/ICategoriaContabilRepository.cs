using SmartGest.Core.Domain;

namespace SmartGest.Application.Abstractions;

public interface ICategoriaContabilRepository
{
    Task<CategoriaContabil?> ObterAsync(int id, CancellationToken cancellationToken = default);
}
