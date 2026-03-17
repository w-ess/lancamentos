namespace Lancamentos.Aplicacao.Services.RegistrarLancamento;

public sealed record RegistrarLancamentoCommand(
    string Tipo,
    decimal Valor,
    DateOnly DataLancamento,
    string CorrelacaoId);
