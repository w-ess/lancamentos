using ConsolidadoDiario.Aplicacao.Abstracoes;

namespace ConsolidadoDiario.Testes.Integracao.Infraestrutura;

public sealed class RelogioUtcFixo : IRelogioUtc
{
    public RelogioUtcFixo(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; }
}
