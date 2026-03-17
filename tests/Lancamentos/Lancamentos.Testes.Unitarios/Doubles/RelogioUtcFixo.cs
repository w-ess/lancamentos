using Lancamentos.Aplicacao.Abstracoes;

namespace Lancamentos.Testes.Unitarios.Doubles;

internal sealed class RelogioUtcFixo : IRelogioUtc
{
    public RelogioUtcFixo(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; }
}
