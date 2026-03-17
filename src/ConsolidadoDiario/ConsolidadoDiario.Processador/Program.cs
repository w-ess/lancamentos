using ConsolidadoDiario.Aplicacao.CasosDeUso.ProcessarLancamentoRegistrado;
using ConsolidadoDiario.Infraestrutura.Configuracao;
using ConsolidadoDiario.Processador;
using ConsolidadoDiario.Processador.Mensageria;

var builder = Host.CreateApplicationBuilder(args);

ConfiguracaoInfraestrutura.AdicionarInfraestruturaConsolidadoDiario(
    builder.Services,
    builder.Configuration);
builder.Services.AddScoped<ProcessarLancamentoRegistradoCasoDeUso>();
builder.Services.AddOptions<RabbitMqOpcoes>()
    .Bind(builder.Configuration.GetSection(RabbitMqOpcoes.Secao))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHostedService<ProcessadorLancamentoRegistrado>();

var host = builder.Build();
host.Run();

public partial class Program;
