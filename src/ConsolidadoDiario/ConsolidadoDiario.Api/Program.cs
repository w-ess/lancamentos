using ConsolidadoDiario.Api.Autenticacao;
using ConsolidadoDiario.Api.Erros;
using ConsolidadoDiario.Aplicacao.Services.ConsultarSaldoDiario;
using ConsolidadoDiario.Infraestrutura.Configuracao;
using ConsolidadoDiario.Infraestrutura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opcoes =>
    {
        opcoes.JsonSerializerOptions.PropertyNamingPolicy = null;
        opcoes.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });

builder.Services.Configure<ApiBehaviorOptions>(opcoes =>
{
    opcoes.InvalidModelStateResponseFactory = contexto =>
    {
        var possuiErroDesserializacao = contexto.ModelState
            .SelectMany(entrada => entrada.Value?.Errors.Select(erro => new { entrada.Key, Erro = erro }) ?? [])
            .Any(item =>
                item.Erro.Exception is not null ||
                item.Key.StartsWith("$", StringComparison.Ordinal) ||
                item.Erro.ErrorMessage.Contains("could not be converted", StringComparison.OrdinalIgnoreCase) ||
                item.Erro.ErrorMessage.Contains("JSON", StringComparison.OrdinalIgnoreCase));

        if (possuiErroDesserializacao)
        {
            return new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Requisicao invalida.",
                Detail = "O corpo da requisicao esta mal formatado.",
                Extensions =
                {
                    ["TraceId"] = contexto.HttpContext.TraceIdentifier
                }
            });
        }

        return new BadRequestObjectResult(new ValidationProblemDetails(contexto.ModelState)
        {
            Status = StatusCodes.Status400BadRequest
        });
    };
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManipuladorExcecoesHttp>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.EnableAnnotations();

    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fluxo de Caixa - API de Consolidado Diario",
        Version = "v1",
        Description = "API responsavel por consultar o saldo diario consolidado."
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
    {
        opcoes.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    opcoes.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {token}."
    });

    opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services.AdicionarInfraestruturaConsolidadoDiario(builder.Configuration);
builder.Services.AdicionarAutenticacaoJwtConsolidadoDiario(builder.Configuration);

var consultaSaldoOpcoes = builder.Configuration
    .GetSection(ConsultarSaldoDiarioOpcoes.Secao)
    .Get<ConsultarSaldoDiarioOpcoes>() ?? new ConsultarSaldoDiarioOpcoes();

_ = consultaSaldoOpcoes.ObterToleranciaDefasagem();

builder.Services.AddSingleton(consultaSaldoOpcoes);
builder.Services.AddScoped<ConsultarSaldoDiarioPorDataService>();

var app = builder.Build();

await app.MigrarBancoDadosConsolidadoDiarioAsync();

app.UseSwagger();
app.UseSwaggerUI(opcoes =>
{
    opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Fluxo de Caixa - Consolidado Diario v1");
    opcoes.RoutePrefix = "swagger";
});
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
