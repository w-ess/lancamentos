using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Aplicacao.Abstracoes;

public interface IOutboxMessageRepository
{
    Task<IReadOnlyCollection<OutboxMessage>> ObterPendentesAsync(
        int quantidadeMaxima,
        CancellationToken cancellationToken = default);

    Task MarcarComoPublicadaAsync(
        Guid mensagemId,
        DateTime publicadaEmUtc,
        CancellationToken cancellationToken = default);

    Task RegistrarFalhaPublicacaoAsync(
        Guid mensagemId,
        string erro,
        CancellationToken cancellationToken = default);
}
