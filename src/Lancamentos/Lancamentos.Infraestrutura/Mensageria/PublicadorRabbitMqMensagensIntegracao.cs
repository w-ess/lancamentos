using System.Text;
using Lancamentos.Dominio.Entidades;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Lancamentos.Infraestrutura.Mensageria;

public sealed class PublicadorRabbitMqMensagensIntegracao :
    IPublicadorMensagensIntegracao,
    IAsyncDisposable
{
    private readonly RabbitMqOpcoes _opcoes;
    private readonly ILogger<PublicadorRabbitMqMensagensIntegracao> _logger;
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    private IConnection? _conexao;
    private IChannel? _canal;

    public PublicadorRabbitMqMensagensIntegracao(
        IOptions<RabbitMqOpcoes> opcoes,
        ILogger<PublicadorRabbitMqMensagensIntegracao> logger)
    {
        ArgumentNullException.ThrowIfNull(opcoes);
        ArgumentNullException.ThrowIfNull(logger);

        _opcoes = opcoes.Value;
        _logger = logger;
    }

    public async Task PublicarAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outboxMessage);

        try
        {
            var canal = await ObterCanalAsync(cancellationToken);
            var propriedades = new BasicProperties
            {
                MessageId = outboxMessage.Id.ToString(),
                CorrelationId = outboxMessage.CorrelacaoId,
                Type = outboxMessage.Tipo,
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(new DateTimeOffset(outboxMessage.OcorridaEmUtc).ToUnixTimeSeconds())
            };

            var corpo = Encoding.UTF8.GetBytes(outboxMessage.Conteudo);
            var routingKey = TopologiaRabbitMq.ObterRoutingKey(outboxMessage.Tipo);

            await canal.BasicPublishAsync(
                exchange: TopologiaRabbitMq.ExchangeEventos,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: propriedades,
                body: corpo,
                cancellationToken: cancellationToken);
        }
        catch
        {
            await ReiniciarCanalAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ReiniciarCanalAsync();
        _semaforo.Dispose();
    }

    private async Task<IChannel> ObterCanalAsync(CancellationToken cancellationToken)
    {
        if (_canal is { IsOpen: true })
        {
            return _canal;
        }

        await _semaforo.WaitAsync(cancellationToken);

        try
        {
            if (_canal is { IsOpen: true })
            {
                return _canal;
            }

            var fabricaConexao = new ConnectionFactory
            {
                HostName = _opcoes.Host,
                Port = _opcoes.Port,
                UserName = _opcoes.Usuario,
                Password = _opcoes.Senha,
                VirtualHost = _opcoes.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                ClientProvidedName = "lancamentos-publicador-mensagens-saida"
            };

            _conexao = await fabricaConexao.CreateConnectionAsync(cancellationToken);
            _canal = await _conexao.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                cancellationToken);

            await DeclararTopologiaAsync(_canal, cancellationToken);

            return _canal;
        }
        finally
        {
            _semaforo.Release();
        }
    }

    private async Task DeclararTopologiaAsync(IChannel canal, CancellationToken cancellationToken)
    {
        await canal.ExchangeDeclareAsync(
            exchange: TopologiaRabbitMq.ExchangeEventos,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await canal.ExchangeDeclareAsync(
            exchange: TopologiaRabbitMq.ExchangeMensagensMortas,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var argumentosFilaPrincipal = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = TopologiaRabbitMq.ExchangeMensagensMortas
        };

        await canal.QueueDeclareAsync(
            queue: TopologiaRabbitMq.FilaLancamentoRegistradoV1,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: argumentosFilaPrincipal,
            cancellationToken: cancellationToken);

        await canal.QueueDeclareAsync(
            queue: TopologiaRabbitMq.FilaLancamentoRegistradoV1Dlq,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await canal.QueueBindAsync(
            queue: TopologiaRabbitMq.FilaLancamentoRegistradoV1,
            exchange: TopologiaRabbitMq.ExchangeEventos,
            routingKey: TopologiaRabbitMq.RoutingKeyLancamentoRegistradoV1,
            cancellationToken: cancellationToken);

        await canal.QueueBindAsync(
            queue: TopologiaRabbitMq.FilaLancamentoRegistradoV1Dlq,
            exchange: TopologiaRabbitMq.ExchangeMensagensMortas,
            routingKey: TopologiaRabbitMq.RoutingKeyLancamentoRegistradoV1,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Topologia RabbitMQ declarada. Exchange={Exchange} Queue={Queue} Dlq={Dlq}",
            TopologiaRabbitMq.ExchangeEventos,
            TopologiaRabbitMq.FilaLancamentoRegistradoV1,
            TopologiaRabbitMq.FilaLancamentoRegistradoV1Dlq);
    }

    private async Task ReiniciarCanalAsync()
    {
        await _semaforo.WaitAsync();

        try
        {
            if (_canal is not null)
            {
                await _canal.DisposeAsync();
                _canal = null;
            }

            if (_conexao is not null)
            {
                await _conexao.DisposeAsync();
                _conexao = null;
            }
        }
        finally
        {
            _semaforo.Release();
        }
    }
}
