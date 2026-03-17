using System.ComponentModel.DataAnnotations;
using Lancamentos.Api.Contratos;
using Lancamentos.Aplicacao.CasosDeUso;
using Lancamentos.Aplicacao.CasosDeUso.ConsultarLancamento;
using Lancamentos.Aplicacao.CasosDeUso.RegistrarLancamento;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lancamentos.Api.Endpoints;

public static class LancamentosEndpoints
{
    public static IEndpointRouteBuilder MapearEndpointsLancamentos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/lancamentos");

        grupo.MapPost("/", RegistrarAsync);
        grupo.MapGet("/{id:guid}", ObterPorIdAsync)
            .WithName("ObterLancamentoPorId");

        return app;
    }

    private static async Task<Results<Created<LancamentoDto>, ValidationProblem>> RegistrarAsync(
        RegistrarLancamentoRequest? request,
        RegistrarLancamentoCasoDeUso casoDeUso,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["CorpoRequisicao"] = new[] { "O corpo da requisicao e obrigatorio." }
            });
        }

        var erros = Validar(request);

        if (erros.Count > 0)
        {
            return TypedResults.ValidationProblem(erros);
        }

        var lancamento = await casoDeUso.ExecutarAsync(request.ParaComando(), cancellationToken);

        return TypedResults.Created($"/api/v1/lancamentos/{lancamento.Id}", lancamento);
    }

    private static async Task<Results<Ok<LancamentoDto>, NotFound>> ObterPorIdAsync(
        Guid id,
        ConsultarLancamentoPorIdCasoDeUso casoDeUso,
        CancellationToken cancellationToken)
    {
        var lancamento = await casoDeUso.ExecutarAsync(id, cancellationToken);

        return lancamento is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(lancamento);
    }

    private static Dictionary<string, string[]> Validar(RegistrarLancamentoRequest request)
    {
        var contexto = new ValidationContext(request);
        var resultados = new List<ValidationResult>();

        Validator.TryValidateObject(request, contexto, resultados, validateAllProperties: true);

        return resultados
            .SelectMany(resultado =>
            {
                var membros = resultado.MemberNames.Any()
                    ? resultado.MemberNames
                    : new[] { string.Empty };

                return membros.Select(membro => new
                {
                    Membro = membro,
                    Mensagem = resultado.ErrorMessage ?? "Valor invalido."
                });
            })
            .GroupBy(item => item.Membro)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(item => item.Mensagem).Distinct().ToArray());
    }
}
