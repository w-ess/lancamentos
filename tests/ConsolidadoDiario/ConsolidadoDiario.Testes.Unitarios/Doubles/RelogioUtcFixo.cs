using ConsolidadoDiario.Aplicacao.Abstracoes;

namespace ConsolidadoDiario.Testes.Unitarios.Doubles;

public sealed class RelogioUtcFixo : IRelogioUtc
{
    public RelogioUtcFixo(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; }
}
