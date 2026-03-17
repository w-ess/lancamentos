using System.Globalization;
using ConsolidadoDiario.Api.Autenticacao;
using ConsolidadoDiario.Aplicacao.CasosDeUso;
using ConsolidadoDiario.Aplicacao.CasosDeUso.ConsultarSaldoDiario;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ConsolidadoDiario.Api.Endpoints;

public static class SaldosDiariosEndpoints
{
    public static IEndpointRouteBuilder MapearEndpointsSaldosDiarios(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/saldos-diarios");

        grupo.MapGet("/{data}", ObterPorDataAsync)
            .RequireAuthorization(PoliticasAutorizacao.ConsolidadoLeitura)
            .WithName("ObterSaldoDiarioPorData");

        return app;
    }

    private static async Task<Results<Ok<SaldoDiarioDto>, ValidationProblem>> ObterPorDataAsync(
        string data,
        ConsultarSaldoDiarioPorDataCasoDeUso casoDeUso,
        CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParseExact(data, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataConsulta))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Data"] = ["A data deve estar no formato yyyy-MM-dd."]
            });
        }

        var saldoDiario = await casoDeUso.ExecutarAsync(dataConsulta, cancellationToken);

        return TypedResults.Ok(saldoDiario);
    }
}
