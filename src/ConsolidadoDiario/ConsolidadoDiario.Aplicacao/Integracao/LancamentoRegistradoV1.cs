namespace ConsolidadoDiario.Aplicacao.Integracao;

public sealed record LancamentoRegistradoV1(
    Guid EventoId,
    DateTime Ocorrido,
    Guid LancamentoId,
    string Tipo,
    decimal Valor,
    DateOnly DataLancamento,
    string CorrelacaoId);
