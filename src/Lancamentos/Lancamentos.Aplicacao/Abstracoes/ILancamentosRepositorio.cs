using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Aplicacao.Abstracoes;

public interface ILancamentosRepositorio
{
    Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default);

    Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
