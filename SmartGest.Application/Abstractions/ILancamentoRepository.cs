using SmartGest.Core.Domain;

namespace SmartGest.Application.Abstractions;

public interface ILancamentoRepository
{
    Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default);
}
