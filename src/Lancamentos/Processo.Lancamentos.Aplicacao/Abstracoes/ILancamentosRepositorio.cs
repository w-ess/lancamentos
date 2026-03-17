using Processo.Lancamentos.Dominio.Entidades;

namespace Processo.Lancamentos.Aplicacao.Abstracoes;

public interface ILancamentosRepositorio
{
    Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default);

    Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
