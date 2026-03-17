using Lancamentos.Dominio.Entidades;
using Lancamentos.Infraestrutura.Mensageria;

namespace Lancamentos.Testes.Integracao.Infraestrutura;

public sealed class PublicadorMensagensIntegracaoFalso : IPublicadorMensagensIntegracao
{
    private readonly object _trava = new();
    private readonly List<MensagemSaida> _mensagensPublicadas = new();
    private int _falhasRestantes;

    public void DefinirFalhasRestantes(int falhasRestantes)
    {
        _falhasRestantes = falhasRestantes;
    }

    public Task PublicarAsync(MensagemSaida mensagemSaida, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mensagemSaida);

        lock (_trava)
        {
            if (_falhasRestantes > 0)
            {
                _falhasRestantes--;
                throw new InvalidOperationException("Falha simulada na publicacao.");
            }

            _mensagensPublicadas.Add(mensagemSaida);
        }

        return Task.CompletedTask;
    }

    public IReadOnlyCollection<MensagemSaida> ListarMensagensPublicadas()
    {
        lock (_trava)
        {
            return _mensagensPublicadas.ToArray();
        }
    }
}
