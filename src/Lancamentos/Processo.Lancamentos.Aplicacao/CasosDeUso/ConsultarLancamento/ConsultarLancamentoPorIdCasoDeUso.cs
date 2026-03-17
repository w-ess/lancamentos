using Processo.Lancamentos.Aplicacao.Abstracoes;

namespace Processo.Lancamentos.Aplicacao.CasosDeUso.ConsultarLancamento;

public sealed class ConsultarLancamentoPorIdCasoDeUso
{
    private readonly ILancamentosRepositorio _repositorio;

    public ConsultarLancamentoPorIdCasoDeUso(ILancamentosRepositorio repositorio)
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

        return lancamento is null ? null : LancamentoDto.DeEntidade(lancamento);
    }
}
