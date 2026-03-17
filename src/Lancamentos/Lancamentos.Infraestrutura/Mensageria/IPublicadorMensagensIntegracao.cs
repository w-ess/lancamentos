using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Infraestrutura.Mensageria;

public interface IPublicadorMensagensIntegracao
{
    Task PublicarAsync(MensagemSaida mensagemSaida, CancellationToken cancellationToken = default);
}
