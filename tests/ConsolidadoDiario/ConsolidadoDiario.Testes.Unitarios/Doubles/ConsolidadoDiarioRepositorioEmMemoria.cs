using ConsolidadoDiario.Aplicacao.Abstracoes;
using ConsolidadoDiario.Dominio.Entidades;

namespace ConsolidadoDiario.Testes.Unitarios.Doubles;

public sealed class ConsolidadoDiarioRepositorioEmMemoria : IConsolidadoDiarioRepositorio
{
    private readonly Dictionary<DateOnly, SaldoDiario> _saldos = [];
    private readonly Dictionary<Guid, LancamentoProcessado> _lancamentosProcessados = [];

    public Task<SaldoDiario?> ObterSaldoPorDataAsync(
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        _saldos.TryGetValue(data, out var saldo);
        return Task.FromResult(saldo);
    }

    public Task AdicionarSaldoAsync(
        SaldoDiario saldoDiario,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(saldoDiario);

        _saldos[saldoDiario.Data] = saldoDiario;
        return Task.CompletedTask;
    }

    public Task<bool> ExisteLancamentoProcessadoAsync(
        Guid lancamentoId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_lancamentosProcessados.ContainsKey(lancamentoId));
    }

    public Task AdicionarLancamentoProcessadoAsync(
        LancamentoProcessado lancamentoProcessado,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lancamentoProcessado);

        _lancamentosProcessados[lancamentoProcessado.LancamentoId] = lancamentoProcessado;
        return Task.CompletedTask;
    }

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }

    public SaldoDiario? ObterSaldo(DateOnly data)
    {
        _saldos.TryGetValue(data, out var saldo);
        return saldo;
    }

    public IReadOnlyCollection<LancamentoProcessado> ListarLancamentosProcessados()
    {
        return _lancamentosProcessados.Values.ToArray();
    }
}
