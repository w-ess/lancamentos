namespace ConsolidadoDiario.Aplicacao.Services.ProcessarLancamentoRegistrado;

public sealed record ResultadoProcessamentoLancamento(
    Guid LancamentoId,
    DateOnly DataLancamento,
    bool JaProcessado);
