using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Lancamentos.Api.Controllers;

[ApiController]
[Tags("Sistema")]
public sealed class SistemaController : ControllerBase
{
    /// <summary>
    /// Retorna um resumo simples do estado da API.
    /// </summary>
    [HttpGet("/")]
    [SwaggerOperation(
        Summary = "Obter status da raiz",
        Description = "Endpoint simples para verificar se a API foi inicializada.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ObterRaiz()
    {
        return Ok(new
        {
            Service = "Lancamentos",
            Status = "Inicializado"
        });
    }

    /// <summary>
    /// Retorna o health check simplificado da API.
    /// </summary>
    [HttpGet("/health")]
    [SwaggerOperation(
        Summary = "Health check",
        Description = "Endpoint anonimo para verificar se a API esta respondendo.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ObterHealth()
    {
        return Ok(new
        {
            Status = "Healthy"
        });
    }
}
