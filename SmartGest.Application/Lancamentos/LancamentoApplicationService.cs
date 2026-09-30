using SmartGest.Application.Abstractions;
using SmartGest.Core.Domain;

namespace SmartGest.Application.Lancamentos;

public sealed class LancamentoApplicationService : ILancamentoApplicationService
{
    private readonly ICategoriaContabilRepository _categorias;
    private readonly ILancamentoRepository _lancamentos;
    private readonly IUnitOfWork _unitOfWork;

    public LancamentoApplicationService(
        ICategoriaContabilRepository categorias,
        ILancamentoRepository lancamentos,
        IUnitOfWork unitOfWork)
    {
        _categorias = categorias;
        _lancamentos = lancamentos;
        _unitOfWork = unitOfWork;
    }

    public async Task<LancamentoResult> CriarAsync(
        CriarLancamentoCommand command,
        CancellationToken cancellationToken = default)
    {
        var categoria = await _categorias.ObterAsync(command.CategoriaId, cancellationToken);

        if (categoria is null)
            throw new DomainException($"Categoria #{command.CategoriaId} não encontrada.");

        if (!categoria.Ativo)
            throw new DomainException($"A categoria '{categoria.Nome}' está inactiva.");

        if (!string.Equals(categoria.Tipo, command.Tipo, StringComparison.Ordinal))
            throw new DomainException(
                $"A categoria '{categoria.Nome}' é do tipo '{categoria.Tipo}' e não pode ser usada num lançamento '{command.Tipo}'.");

        var lancamento = new Lancamento(
            command.Data,
            command.Descricao,
            command.Tipo,
            command.Valor,
            categoria.Id,
            command.Beneficiario,
            command.MetodoPagamento,
            command.CaminhoDocumento,
            command.Observacoes,
            command.CentroCusto,
            command.ReferenciaInterna,
            command.ContaBancariaId);

        lancamento.DefinirCategoria(categoria.Nome);

        await _lancamentos.AdicionarAsync(lancamento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LancamentoResult(
            lancamento.Id,
            lancamento.Data,
            lancamento.Descricao,
            lancamento.Tipo,
            lancamento.Valor,
            categoria.Id,
            categoria.Nome);
    }
}
