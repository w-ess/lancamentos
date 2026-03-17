using ConsolidadoDiario.Aplicacao.Abstracoes;

namespace ConsolidadoDiario.Infraestrutura.Servicos;

public sealed class RelogioSistemaUtc : IRelogioUtc
{
    public DateTime UtcNow => DateTime.UtcNow;
}
