using System.Globalization;
using ConsolidadoDiario.Api.Autenticacao;
using ConsolidadoDiario.Aplicacao.Services;
using ConsolidadoDiario.Aplicacao.Services.ConsultarSaldoDiario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ConsolidadoDiario.Api.Controllers;

[ApiController]
[Route("api/v1/saldos-diarios")]
[Tags("SaldosDiarios")]
public sealed class SaldosDiariosController : ControllerBase
{
    /// <summary>
    /// Consulta o saldo diario consolidado para a data informada.
    /// </summary>
    /// <remarks>
    /// A data deve ser informada no formato <c>yyyy-MM-dd</c>.
    /// </remarks>
    [HttpGet("{data}")]
    [Authorize(Policy = PoliticasAutorizacao.ConsolidadoLeitura)]
    [Produces("application/json")]
    [SwaggerOperation(
        Summary = "Consultar saldo diario",
        Description = "Retorna o total de creditos, debitos e saldo consolidado de uma data especifica.")]
    [ProducesResponseType<SaldoDiarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SaldoDiarioDto>> ObterPorDataAsync(
        [SwaggerParameter("Data no formato yyyy-MM-dd. Exemplo: 2026-03-17.", Required = true)]
        string data,
        [FromServices] ConsultarSaldoDiarioPorDataService service,
        CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParseExact(data, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataConsulta))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Data"] = ["A data deve estar no formato yyyy-MM-dd."]
            })
            {
                Status = StatusCodes.Status400BadRequest
            });
        }

        var saldoDiario = await service.ExecutarAsync(dataConsulta, cancellationToken);

        return Ok(saldoDiario);
    }
}
