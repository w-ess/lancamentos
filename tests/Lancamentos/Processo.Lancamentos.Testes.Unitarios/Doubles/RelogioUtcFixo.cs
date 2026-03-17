using Processo.Lancamentos.Aplicacao.Abstracoes;

namespace Processo.Lancamentos.Testes.Unitarios.Doubles;

internal sealed class RelogioUtcFixo : IRelogioUtc
{
    public RelogioUtcFixo(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; }
}
