using Lancamentos.Dominio.Entidades;
using Lancamentos.Infraestrutura.Mensageria;

namespace Lancamentos.Testes.Integracao.Infraestrutura;

public sealed class PublicadorMensagensIntegracaoFalso : IPublicadorMensagensIntegracao
{
    private readonly object _trava = new();
    private readonly List<OutboxMessage> _mensagensPublicadas = new();
    private int _falhasRestantes;

    public void DefinirFalhasRestantes(int falhasRestantes)
    {
        _falhasRestantes = falhasRestantes;
    }

    public Task PublicarAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outboxMessage);

        lock (_trava)
        {
            if (_falhasRestantes > 0)
            {
                _falhasRestantes--;
                throw new InvalidOperationException("Falha simulada na publicacao.");
            }

            _mensagensPublicadas.Add(outboxMessage);
        }

        return Task.CompletedTask;
    }

    public IReadOnlyCollection<OutboxMessage> ListarMensagensPublicadas()
    {
        lock (_trava)
        {
            return _mensagensPublicadas.ToArray();
        }
    }
}
