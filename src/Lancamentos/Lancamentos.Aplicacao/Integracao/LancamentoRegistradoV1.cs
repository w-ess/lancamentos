using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Aplicacao.Integracao;

public sealed record LancamentoRegistradoV1(
    Guid EventoId,
    DateTime OcorridoEmUtc,
    Guid LancamentoId,
    string Tipo,
    decimal Valor,
    DateOnly DataLancamento,
    string CorrelacaoId)
{
    public static LancamentoRegistradoV1 Criar(Lancamento lancamento, Guid eventoId, string correlacaoId)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        return new LancamentoRegistradoV1(
            eventoId,
            lancamento.RegistradoEmUtc,
            lancamento.Id,
            lancamento.Tipo.Valor,
            lancamento.Valor.Valor,
            lancamento.DataLancamento.Valor,
            correlacaoId);
    }
}
