using Lancamentos.Aplicacao.Abstracoes;
using Lancamentos.Dominio.Entidades;
using Lancamentos.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Lancamentos.Infraestrutura.Repositorios;

public sealed class MensagensSaidaRepositorio : IMensagensSaidaRepositorio
{
    private readonly LancamentosDbContext _dbContext;

    public MensagensSaidaRepositorio(LancamentosDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<MensagemSaida>> ObterPendentesAsync(
        int quantidadeMaxima,
        CancellationToken cancellationToken = default)
    {
        if (quantidadeMaxima <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantidadeMaxima));
        }

        return await _dbContext.MensagensSaida
            .AsNoTracking()
            .Where(mensagem => mensagem.PublicadaEmUtc == null)
            .OrderBy(mensagem => mensagem.OcorridaEmUtc)
            .ThenBy(mensagem => mensagem.Id)
            .Take(quantidadeMaxima)
            .ToArrayAsync(cancellationToken);
    }

    public async Task MarcarComoPublicadaAsync(
        Guid mensagemId,
        DateTime publicadaEmUtc,
        CancellationToken cancellationToken = default)
    {
        var mensagem = await ObterPorIdAsync(mensagemId, cancellationToken);
        mensagem.MarcarComoPublicada(publicadaEmUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RegistrarFalhaPublicacaoAsync(
        Guid mensagemId,
        string erro,
        CancellationToken cancellationToken = default)
    {
        var mensagem = await ObterPorIdAsync(mensagemId, cancellationToken);
        mensagem.RegistrarFalhaPublicacao(erro);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<MensagemSaida> ObterPorIdAsync(Guid mensagemId, CancellationToken cancellationToken)
    {
        return await _dbContext.MensagensSaida
            .SingleAsync(mensagem => mensagem.Id == mensagemId, cancellationToken);
    }
}
