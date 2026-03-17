using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Aplicacao.Abstracoes;

public interface IRegistroLancamentoRepositorio
{
    Task RegistrarAsync(
        Lancamento lancamento,
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken = default);
}
