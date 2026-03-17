using Lancamentos.Aplicacao.Abstracoes;
using Lancamentos.Dominio.Entidades;
using Lancamentos.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Lancamentos.Infraestrutura.Repositorios;

public sealed class LancamentosRepositorio : ILancamentosRepositorio, IRegistroLancamentoRepositorio
{
    private readonly LancamentosDbContext _dbContext;

    public LancamentosRepositorio(LancamentosDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task AdicionarAsync(
        Lancamento lancamento,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        await _dbContext.Lancamentos.AddAsync(lancamento, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RegistrarAsync(
        Lancamento lancamento,
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lancamento);
        ArgumentNullException.ThrowIfNull(outboxMessage);

        await _dbContext.Lancamentos.AddAsync(lancamento, cancellationToken);
        await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Lancamento?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Lancamentos
            .AsNoTracking()
            .SingleOrDefaultAsync(lancamento => lancamento.Id == id, cancellationToken);
    }
}
