using Lancamentos.Aplicacao.Abstracoes;
using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Testes.Unitarios.Doubles;

internal sealed class LancamentosRepositorioEmMemoria :
    ILancamentosRepositorio,
    IRegistroLancamentoRepositorio,
    IMensagensSaidaRepositorio
{
    private readonly Dictionary<Guid, Lancamento> _itens = new();
    private readonly Dictionary<Guid, MensagemSaida> _mensagens = new();

    public Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        _itens[lancamento.Id] = lancamento;
        return Task.CompletedTask;
    }

    public Task RegistrarAsync(
        Lancamento lancamento,
        MensagemSaida mensagemSaida,
        CancellationToken cancellationToken = default)
    {
        _itens[lancamento.Id] = lancamento;
        _mensagens[mensagemSaida.Id] = mensagemSaida;
        return Task.CompletedTask;
    }

    public Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _itens.TryGetValue(id, out var lancamento);
        return Task.FromResult(lancamento);
    }

    public Task<IReadOnlyCollection<MensagemSaida>> ObterPendentesAsync(
        int quantidadeMaxima,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<MensagemSaida> mensagens = _mensagens.Values
            .Where(mensagem => !mensagem.Publicada)
            .Take(quantidadeMaxima)
            .ToArray();

        return Task.FromResult(mensagens);
    }

    public Task MarcarComoPublicadaAsync(
        Guid mensagemId,
        DateTime publicadaEmUtc,
        CancellationToken cancellationToken = default)
    {
        _mensagens[mensagemId].MarcarComoPublicada(publicadaEmUtc);
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

    public IReadOnlyCollection<MensagemSaida> ListarMensagens() => _mensagens.Values.ToArray();
}
