using ConsolidadoDiario.Api.Autenticacao;
using ConsolidadoDiario.Api.Endpoints;
using ConsolidadoDiario.Api.Erros;
using ConsolidadoDiario.Aplicacao.CasosDeUso.ConsultarSaldoDiario;
using ConsolidadoDiario.Infraestrutura.Configuracao;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(opcoes =>
{
    opcoes.SerializerOptions.PropertyNamingPolicy = null;
    opcoes.SerializerOptions.DictionaryKeyPolicy = null;
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManipuladorExcecoesHttp>();

builder.Services.AdicionarInfraestruturaConsolidadoDiario(builder.Configuration);
builder.Services.AdicionarAutenticacaoJwtConsolidadoDiario(builder.Configuration);

var consultaSaldoOpcoes = builder.Configuration
    .GetSection(ConsultarSaldoDiarioOpcoes.Secao)
    .Get<ConsultarSaldoDiarioOpcoes>() ?? new ConsultarSaldoDiarioOpcoes();

_ = consultaSaldoOpcoes.ObterToleranciaDefasagem();

builder.Services.AddSingleton(consultaSaldoOpcoes);
builder.Services.AddScoped<ConsultarSaldoDiarioPorDataCasoDeUso>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePages(async contextoStatusCode =>
{
    if (contextoStatusCode.HttpContext.Response.StatusCode != StatusCodes.Status400BadRequest)
    {
        return;
    }

    var problemDetailsService = contextoStatusCode.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

    await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
    {
        HttpContext = contextoStatusCode.HttpContext,
        ProblemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Requisicao invalida.",
            Detail = "O corpo da requisicao esta mal formatado.",
            Extensions =
            {
                ["TraceId"] = contextoStatusCode.HttpContext.TraceIdentifier
            }
        }
    });
});

app.MapGet("/", () => Results.Ok(new
{
    Servico = "ConsolidadoDiario",
    Status = "Inicializado"
}));

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy"
}));

app.MapearEndpointsSaldosDiarios();

app.Run();

public partial class Program;
