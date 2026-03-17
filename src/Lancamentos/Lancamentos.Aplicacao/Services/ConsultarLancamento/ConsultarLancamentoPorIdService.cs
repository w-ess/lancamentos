using Lancamentos.Aplicacao.Abstracoes;

namespace Lancamentos.Aplicacao.Services.ConsultarLancamento;

public sealed class ConsultarLancamentoPorIdService
{
    private readonly ILancamentosRepositorio _repositorio;

    public ConsultarLancamentoPorIdService(ILancamentosRepositorio repositorio)
    {
        ArgumentNullException.ThrowIfNull(repositorio);

        _repositorio = repositorio;
    }

    public async Task<LancamentoDto?> ExecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador do lancamento e obrigatorio.", nameof(id));
        }

        var lancamento = await _repositorio.ObterPorIdAsync(id, cancellationToken);

        return lancamento is null ? null : LancamentoDto.FromEntity(lancamento);
    }
}
