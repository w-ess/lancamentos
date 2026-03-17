using Lancamentos.Api.Autenticacao;
using Lancamentos.Api.Contratos;
using Lancamentos.Aplicacao.Services;
using Lancamentos.Aplicacao.Services.ConsultarLancamento;
using Lancamentos.Aplicacao.Services.RegistrarLancamento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Lancamentos.Api.Controllers;

[ApiController]
[Route("api/v1/lancamentos")]
[Tags("Lancamentos")]
public sealed class LancamentosController : ControllerBase
{
    /// <summary>
    /// Registra um novo lancamento de credito ou debito.
    /// </summary>
    /// <remarks>
    /// Requer o escopo <c>lancamentos.escrita</c>. O header opcional <c>X-Correlation-Id</c> pode ser enviado para rastreamento.
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.LancamentosEscrita)]
    [Consumes("application/json")]
    [Produces("application/json")]
    [SwaggerOperation(
        Summary = "Registrar lancamento",
        Description = "Cria um novo lancamento e retorna os dados persistidos com o identificador gerado.")]
    [ProducesResponseType<LancamentoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LancamentoDto>> RegistrarAsync(
        [FromBody] RegistrarLancamentoRequest request,
        [FromServices] RegistrarLancamentoService service,
        CancellationToken cancellationToken)
    {
        var correlacaoId = ObterCorrelacaoId(HttpContext);
        var lancamento = await service.ExecutarAsync(request.ToCommand(correlacaoId), cancellationToken);

        return Created($"/api/v1/lancamentos/{lancamento.Id}", lancamento);
    }

    /// <summary>
    /// Consulta um lancamento pelo identificador.
    /// </summary>
    /// <remarks>
    /// Requer o escopo <c>lancamentos.leitura</c>.
    /// </remarks>
    [HttpGet("{id:guid}", Name = "ObterLancamentoPorId")]
    [Authorize(Policy = PoliticasAutorizacao.LancamentosLeitura)]
    [Produces("application/json")]
    [SwaggerOperation(
        Summary = "Consultar lancamento por id",
        Description = "Retorna um lancamento existente a partir do identificador informado.")]
    [ProducesResponseType<LancamentoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LancamentoDto>> ObterPorIdAsync(
        [SwaggerParameter("Identificador do lancamento no formato GUID.", Required = true)]
        Guid id,
        [FromServices] ConsultarLancamentoPorIdService service,
        CancellationToken cancellationToken)
    {
        var lancamento = await service.ExecutarAsync(id, cancellationToken);

        return lancamento is null
            ? NotFound()
            : Ok(lancamento);
    }

    private static string ObterCorrelacaoId(HttpContext httpContext)
    {
        const string nomeCabecalho = "X-Correlation-Id";

        if (httpContext.Request.Headers.TryGetValue(nomeCabecalho, out var valores) &&
            !string.IsNullOrWhiteSpace(valores.ToString()))
        {
            return valores.ToString().Trim();
        }

        return httpContext.TraceIdentifier;
    }
}
