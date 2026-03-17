using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Infraestrutura.Mensageria;

public interface IPublicadorMensagensIntegracao
{
    Task PublicarAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default);
}
