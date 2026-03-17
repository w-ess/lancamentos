using Lancamentos.Aplicacao.Abstracoes;
using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Testes.Unitarios.Doubles;

internal sealed class LancamentosRepositorioEmMemoria : ILancamentosRepositorio
{
    private readonly Dictionary<Guid, Lancamento> _itens = new();

    public Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        _itens[lancamento.Id] = lancamento;
        return Task.CompletedTask;
    }

    public Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _itens.TryGetValue(id, out var lancamento);
        return Task.FromResult(lancamento);
    }
}
