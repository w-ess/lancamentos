using ConsolidadoDiario.Aplicacao.Abstracoes;
using ConsolidadoDiario.Dominio.Entidades;
using ConsolidadoDiario.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ConsolidadoDiario.Infraestrutura.Repositorios;

public sealed class ConsolidadoDiarioRepositorio : IConsolidadoDiarioRepositorio
{
    private readonly ConsolidadoDiarioDbContext _dbContext;

    public ConsolidadoDiarioRepositorio(ConsolidadoDiarioDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public Task<SaldoDiario?> ObterSaldoPorDataAsync(
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaldosDiarios
            .SingleOrDefaultAsync(saldo => saldo.Data == data, cancellationToken);
    }

    public async Task AdicionarSaldoAsync(
        SaldoDiario saldoDiario,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(saldoDiario);

        await _dbContext.SaldosDiarios.AddAsync(saldoDiario, cancellationToken);
    }

    public Task<bool> ExisteLancamentoProcessadoAsync(
        Guid lancamentoId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LancamentosProcessados
            .AsNoTracking()
            .AnyAsync(lancamento => lancamento.LancamentoId == lancamentoId, cancellationToken);
    }

    public async Task AdicionarLancamentoProcessadoAsync(
        LancamentoProcessado lancamentoProcessado,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lancamentoProcessado);

        await _dbContext.LancamentosProcessados.AddAsync(lancamentoProcessado, cancellationToken);
    }

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
