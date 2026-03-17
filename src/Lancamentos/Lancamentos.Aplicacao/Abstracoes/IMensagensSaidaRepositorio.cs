using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Aplicacao.Abstracoes;

public interface IMensagensSaidaRepositorio
{
    Task<IReadOnlyCollection<MensagemSaida>> ObterPendentesAsync(
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
