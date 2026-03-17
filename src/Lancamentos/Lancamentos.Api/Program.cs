using Lancamentos.Api.Autenticacao;
using Lancamentos.Api.Endpoints;
using Lancamentos.Api.Erros;
using Lancamentos.Aplicacao.CasosDeUso.ConsultarLancamento;
using Lancamentos.Aplicacao.CasosDeUso.RegistrarLancamento;
using Lancamentos.Infraestrutura.Configuracao;
using Lancamentos.Infraestrutura.Persistencia;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(opcoes =>
{
    opcoes.SerializerOptions.PropertyNamingPolicy = null;
    opcoes.SerializerOptions.DictionaryKeyPolicy = null;
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManipuladorExcecoesHttp>();

builder.Services.AdicionarInfraestruturaLancamentos(builder.Configuration);
builder.Services.AdicionarAutenticacaoJwtLancamentos(builder.Configuration);
builder.Services.AddScoped<RegistrarLancamentoCasoDeUso>();
builder.Services.AddScoped<ConsultarLancamentoPorIdCasoDeUso>();

var app = builder.Build();

await app.MigrarBancoDadosLancamentosAsync();

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
    Servico = "Lancamentos",
    Status = "Inicializado"
}));

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy"
}));

app.MapearEndpointsLancamentos();

app.Run();

public partial class Program;
