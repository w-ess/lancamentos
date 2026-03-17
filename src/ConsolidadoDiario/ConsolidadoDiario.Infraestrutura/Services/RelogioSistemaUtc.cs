using ConsolidadoDiario.Aplicacao.Abstracoes;

namespace ConsolidadoDiario.Infraestrutura.Services;

public sealed class RelogioSistemaUtc : IRelogioUtc
{
    public DateTime UtcNow => DateTime.UtcNow;
}
