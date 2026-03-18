using Lancamentos.Aplicacao.Abstracoes;
using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Testes.Unitarios.Doubles;

internal sealed class LancamentosRepositorioEmMemoria :
    ILancamentosRepositorio,
    IRegistroLancamentoRepositorio,
    IOutboxMessageRepository
{
    private readonly Dictionary<Guid, Lancamento> _itens = new();
    private readonly Dictionary<Guid, OutboxMessage> _mensagens = new();

    public Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        _itens[lancamento.Id] = lancamento;
        return Task.CompletedTask;
    }

    public Task RegistrarAsync(
        Lancamento lancamento,
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken = default)
    {
        _itens[lancamento.Id] = lancamento;
        _mensagens[outboxMessage.Id] = outboxMessage;
        return Task.CompletedTask;
    }

    public Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _itens.TryGetValue(id, out var lancamento);
        return Task.FromResult(lancamento);
    }

    public Task<IReadOnlyCollection<OutboxMessage>> ObterPendentesAsync(
        int quantidadeMaxima,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<OutboxMessage> mensagens = _mensagens.Values
            .Where(mensagem => !mensagem.EstaPublicada)
            .Take(quantidadeMaxima)
            .ToArray();

        return Task.FromResult(mensagens);
    }

    public Task MarcarComoPublicadaAsync(
        Guid mensagemId,
        DateTime publicada,
        CancellationToken cancellationToken = default)
    {
        _mensagens[mensagemId].MarcarComoPublicada(publicada);
        return Task.CompletedTask;
    }

    public Task RegistrarFalhaPublicacaoAsync(
        Guid mensagemId,
        string erro,
        CancellationToken cancellationToken = default)
    {
        _mensagens[mensagemId].RegistrarFalhaPublicacao(erro);
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<OutboxMessage> ListarMensagens() => _mensagens.Values.ToArray();
}
