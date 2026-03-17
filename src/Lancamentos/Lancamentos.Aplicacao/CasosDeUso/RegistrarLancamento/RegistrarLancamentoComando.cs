namespace Lancamentos.Aplicacao.CasosDeUso.RegistrarLancamento;

public sealed record RegistrarLancamentoComando(
    string Tipo,
    decimal Valor,
    DateOnly DataLancamento,
    string CorrelacaoId);
