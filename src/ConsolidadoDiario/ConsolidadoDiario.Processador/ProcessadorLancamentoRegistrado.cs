namespace ConsolidadoDiario.Processador;

public sealed class ProcessadorLancamentoRegistrado : BackgroundService
{
    private readonly ILogger<ProcessadorLancamentoRegistrado> _logger;

    public ProcessadorLancamentoRegistrado(ILogger<ProcessadorLancamentoRegistrado> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Processador de consolidado inicializado e aguardando implementacao do consumo de eventos.");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
