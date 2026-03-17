using System.Text;
using System.Text.Json;
using ConsolidadoDiario.Aplicacao.CasosDeUso.ProcessarLancamentoRegistrado;
using ConsolidadoDiario.Aplicacao.Integracao;
using ConsolidadoDiario.Processador.Mensageria;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsolidadoDiario.Processador;

public sealed class ProcessadorLancamentoRegistrado :
    BackgroundService,
    IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOpcoes _opcoes;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProcessadorLancamentoRegistrado> _logger;
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    private IConnection? _conexao;
    private IChannel? _canal;
    private string? _consumerTag;

    public ProcessadorLancamentoRegistrado(
        IOptions<RabbitMqOpcoes> opcoes,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProcessadorLancamentoRegistrado> logger)
    {
        ArgumentNullException.ThrowIfNull(opcoes);
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _opcoes = opcoes.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var canal = await ObterCanalAsync(stoppingToken);

        await canal.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumidor = new AsyncEventingBasicConsumer(canal);
        consumidor.ReceivedAsync += (_, args) => ProcessarMensagemAsync(canal, args, stoppingToken);

        _consumerTag = await canal.BasicConsumeAsync(
            queue: TopologiaRabbitMq.FilaLancamentoRegistradoV1,
            autoAck: false,
            consumer: consumidor,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Consumo da fila {Fila} iniciado com sucesso.",
            TopologiaRabbitMq.FilaLancamentoRegistradoV1);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_canal is { IsOpen: true } && !string.IsNullOrWhiteSpace(_consumerTag))
        {
            await _canal.BasicCancelAsync(_consumerTag, noWait: false, cancellationToken: cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await ReiniciarCanalAsync();
        _semaforo.Dispose();
    }

    private async Task ProcessarMensagemAsync(
        IChannel canal,
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        LancamentoRegistradoV1? evento = null;

        try
        {
            evento = JsonSerializer.Deserialize<LancamentoRegistradoV1>(
                Encoding.UTF8.GetString(args.Body.ToArray()),
                JsonSerializerOptions);

            if (evento is null)
            {
                throw new JsonException("Nao foi possivel desserializar a mensagem de lancamento registrado.");
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var casoDeUso = scope.ServiceProvider.GetRequiredService<ProcessarLancamentoRegistradoCasoDeUso>();

            var resultado = await casoDeUso.ExecutarAsync(evento, cancellationToken);

            if (resultado.JaProcessado)
            {
                _logger.LogInformation(
                    "Lancamento ignorado por idempotencia. LancamentoId={LancamentoId} EventoId={EventoId} CorrelacaoId={CorrelacaoId}",
                    evento.LancamentoId,
                    evento.EventoId,
                    evento.CorrelacaoId);
            }
            else
            {
                _logger.LogInformation(
                    "Lancamento consolidado com sucesso. LancamentoId={LancamentoId} EventoId={EventoId} CorrelacaoId={CorrelacaoId} Data={DataLancamento}",
                    evento.LancamentoId,
                    evento.EventoId,
                    evento.CorrelacaoId,
                    evento.DataLancamento);
            }

            await canal.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Falha ao processar mensagem de lancamento registrado. DeliveryTag={DeliveryTag} EventoId={EventoId} LancamentoId={LancamentoId}",
                args.DeliveryTag,
                evento?.EventoId,
                evento?.LancamentoId);

            if (canal.IsOpen)
            {
                await canal.BasicNackAsync(
                    args.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: cancellationToken);
            }
        }
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
                ClientProvidedName = "consolidado-diario-processador-lancamento-registrado"
            };

            _conexao = await fabricaConexao.CreateConnectionAsync(cancellationToken);
            _canal = await _conexao.CreateChannelAsync(cancellationToken: cancellationToken);

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

            _consumerTag = null;
        }
        finally
        {
            _semaforo.Release();
        }
    }
}
