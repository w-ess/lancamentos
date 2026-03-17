namespace ConsolidadoDiario.Aplicacao.CasosDeUso.ProcessarLancamentoRegistrado;

public sealed record ResultadoProcessamentoLancamento(
    Guid LancamentoId,
    DateOnly DataLancamento,
    bool JaProcessado);
