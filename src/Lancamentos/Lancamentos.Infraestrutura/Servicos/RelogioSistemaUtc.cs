using Lancamentos.Aplicacao.Abstracoes;

namespace Lancamentos.Infraestrutura.Servicos;

public sealed class RelogioSistemaUtc : IRelogioUtc
{
    public DateTime UtcNow => DateTime.UtcNow;
}
