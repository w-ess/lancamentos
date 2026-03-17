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
        MensagemSaida mensagemSaida,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lancamento);
        ArgumentNullException.ThrowIfNull(mensagemSaida);

        await _dbContext.Lancamentos.AddAsync(lancamento, cancellationToken);
        await _dbContext.MensagensSaida.AddAsync(mensagemSaida, cancellationToken);
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
