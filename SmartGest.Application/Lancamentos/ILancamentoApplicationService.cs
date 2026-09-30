namespace SmartGest.Application.Lancamentos;

public interface ILancamentoApplicationService
{
    Task<LancamentoResult> CriarAsync(
        CriarLancamentoCommand command,
        CancellationToken cancellationToken = default);
}
