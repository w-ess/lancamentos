using ConsolidadoDiario.Dominio.Entidades;

namespace ConsolidadoDiario.Aplicacao.Abstracoes;

public interface IConsolidadoDiarioRepositorio
{
    Task<SaldoDiario?> ObterSaldoPorDataAsync(
        DateOnly data,
        CancellationToken cancellationToken = default);

    Task AdicionarSaldoAsync(
        SaldoDiario saldoDiario,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteLancamentoProcessadoAsync(
        Guid lancamentoId,
        CancellationToken cancellationToken = default);

    Task AdicionarLancamentoProcessadoAsync(
        LancamentoProcessado lancamentoProcessado,
        CancellationToken cancellationToken = default);

    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
