using System.Text.Json;
using ConsolidadoDiario.Dominio.Excecoes;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ConsolidadoDiario.Api.Erros;

public sealed class ManipuladorExcecoesHttp : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ManipuladorExcecoesHttp> _logger;

    public ManipuladorExcecoesHttp(
        IProblemDetailsService problemDetailsService,
        ILogger<ManipuladorExcecoesHttp> logger)
    {
        ArgumentNullException.ThrowIfNull(problemDetailsService);
        ArgumentNullException.ThrowIfNull(logger);

        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CriarProblemDetails(httpContext, exception);

        if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Falha inesperada ao processar a requisicao {TraceId}.", httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(exception, "Falha de validacao ao processar a requisicao {TraceId}.", httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static ProblemDetails CriarProblemDetails(HttpContext httpContext, Exception exception)
    {
        var (status, titulo, detalhe) = exception switch
        {
            ExcecaoDominio => (
                StatusCodes.Status400BadRequest,
                "Requisicao invalida.",
                exception.Message),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Requisicao invalida.",
                exception.Message),
            BadHttpRequestException => (
                StatusCodes.Status400BadRequest,
                "Requisicao invalida.",
                exception.Message),
            JsonException => (
                StatusCodes.Status400BadRequest,
                "Requisicao invalida.",
                "O corpo da requisicao esta mal formatado."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno.",
                "Ocorreu um erro inesperado ao processar a requisicao.")
        };

        return new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = detalhe,
            Extensions =
            {
                ["TraceId"] = httpContext.TraceIdentifier
            }
        };
    }
}
