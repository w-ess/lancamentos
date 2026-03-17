using Processo.Lancamentos.Dominio.Entidades;

namespace Processo.Lancamentos.Aplicacao.CasosDeUso;

public sealed record LancamentoDto(
    Guid Id,
    string Tipo,
    decimal Valor,
    DateOnly DataLancamento,
    DateTime RegistradoEmUtc)
{
    public static LancamentoDto DeEntidade(Lancamento lancamento)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        return new LancamentoDto(
            lancamento.Id,
            lancamento.Tipo.Valor,
            lancamento.Valor.Valor,
            lancamento.DataLancamento.Valor,
            lancamento.RegistradoEmUtc);
    }
}
