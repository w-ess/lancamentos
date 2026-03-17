using ConsolidadoDiario.Processador;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ProcessadorLancamentoRegistrado>();

var host = builder.Build();
host.Run();

public partial class Program;
