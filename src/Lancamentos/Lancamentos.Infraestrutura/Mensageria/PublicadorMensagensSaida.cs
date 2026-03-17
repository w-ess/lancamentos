using Lancamentos.Aplicacao.Abstracoes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lancamentos.Infraestrutura.Mensageria;

public sealed class PublicadorMensagensSaida : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IRelogioUtc _relogioUtc;
    private readonly IPublicadorMensagensIntegracao _publicadorMensagensIntegracao;
    private readonly PublicadorMensagensSaidaOpcoes _opcoes;
    private readonly ILogger<PublicadorMensagensSaida> _logger;

    public PublicadorMensagensSaida(
        IServiceScopeFactory serviceScopeFactory,
        IRelogioUtc relogioUtc,
        IPublicadorMensagensIntegracao publicadorMensagensIntegracao,
        IOptions<PublicadorMensagensSaidaOpcoes> opcoes,
        ILogger<PublicadorMensagensSaida> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(relogioUtc);
        ArgumentNullException.ThrowIfNull(publicadorMensagensIntegracao);
        ArgumentNullException.ThrowIfNull(opcoes);
        ArgumentNullException.ThrowIfNull(logger);

        _serviceScopeFactory = serviceScopeFactory;
        _relogioUtc = relogioUtc;
        _publicadorMensagensIntegracao = publicadorMensagensIntegracao;
        _opcoes = opcoes.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Publicador de mensagens de saida iniciado. IntervaloMs={IntervaloMs} Lote={Lote}",
            _opcoes.IntervaloEmMilissegundos,
            _opcoes.QuantidadePorLote);

        if (_opcoes.AtrasoInicialEmMilissegundos > 0)
        {
            await Task.Delay(_opcoes.AtrasoInicialEmMilissegundos, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublicarPendentesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha inesperada no ciclo do publicador de mensagens de saida.");
            }

            await Task.Delay(_opcoes.IntervaloEmMilissegundos, stoppingToken);
        }
    }

    private async Task PublicarPendentesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IMensagensSaidaRepositorio>();
        var mensagensPendentes = await repositorio.ObterPendentesAsync(_opcoes.QuantidadePorLote, cancellationToken);

        foreach (var mensagem in mensagensPendentes)
        {
            try
            {
                await _publicadorMensagensIntegracao.PublicarAsync(mensagem, cancellationToken);
                await repositorio.MarcarComoPublicadaAsync(mensagem.Id, _relogioUtc.UtcNow, cancellationToken);

                _logger.LogInformation(
                    "Mensagem de saida publicada. MensagemId={MensagemId} Tipo={Tipo} CorrelacaoId={CorrelacaoId}",
                    mensagem.Id,
                    mensagem.Tipo,
                    mensagem.CorrelacaoId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                await repositorio.RegistrarFalhaPublicacaoAsync(mensagem.Id, ex.Message, cancellationToken);

                _logger.LogWarning(
                    ex,
                    "Falha ao publicar mensagem de saida. MensagemId={MensagemId} Tipo={Tipo} CorrelacaoId={CorrelacaoId}",
                    mensagem.Id,
                    mensagem.Tipo,
                    mensagem.CorrelacaoId);
            }
        }
    }
}
