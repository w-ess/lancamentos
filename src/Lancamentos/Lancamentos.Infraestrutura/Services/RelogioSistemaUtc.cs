using Lancamentos.Aplicacao.Abstracoes;

namespace Lancamentos.Infraestrutura.Services;

public sealed class RelogioSistemaUtc : IRelogioUtc
{
    public DateTime UtcNow => DateTime.UtcNow;
}
