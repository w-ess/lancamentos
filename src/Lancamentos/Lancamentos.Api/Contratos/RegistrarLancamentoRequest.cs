using System.ComponentModel.DataAnnotations;
using Lancamentos.Aplicacao.Services.RegistrarLancamento;
using Swashbuckle.AspNetCore.Annotations;

namespace Lancamentos.Api.Contratos;

/// <summary>
/// Corpo da requisicao para registrar um novo lancamento.
/// </summary>
public sealed class RegistrarLancamentoRequest : IValidatableObject
{
    /// <summary>
    /// Tipo do lancamento. Valores aceitos: Credito ou Debito.
    /// </summary>
    [Required]
    [SwaggerSchema(Description = "Tipo do lancamento. Valores aceitos: Credito ou Debito.")]
    public string? Tipo { get; init; }

    /// <summary>
    /// Valor monetario do lancamento.
    /// </summary>
    [Required]
    [SwaggerSchema(Description = "Valor do lancamento. Deve ser maior que zero e ter no maximo duas casas decimais.")]
    public decimal? Valor { get; init; }

    /// <summary>
    /// Data do lancamento no formato yyyy-MM-dd.
    /// </summary>
    [Required]
    [SwaggerSchema(Description = "Data do lancamento no formato yyyy-MM-dd.", Format = "date")]
    public DateOnly? DataLancamento { get; init; }

    public RegistrarLancamentoCommand ToCommand(string correlacaoId)
    {
        return new RegistrarLancamentoCommand(
            Tipo!,
            Valor!.Value,
            DataLancamento!.Value,
            correlacaoId);
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Tipo))
        {
            yield return new ValidationResult(
                "O campo Tipo e obrigatorio.",
                new[] { nameof(Tipo) });
        }

        if (Valor is null)
        {
            yield return new ValidationResult(
                "O campo Valor e obrigatorio.",
                new[] { nameof(Valor) });
        }
        else
        {
            if (Valor.Value <= 0)
            {
                yield return new ValidationResult(
                    "O campo Valor deve ser maior que zero.",
                    new[] { nameof(Valor) });
            }

            if (ObterCasasDecimais(Valor.Value) > 2)
            {
                yield return new ValidationResult(
                    "O campo Valor deve ter no maximo duas casas decimais.",
                    new[] { nameof(Valor) });
            }
        }

        if (DataLancamento is null || DataLancamento.Value == default)
        {
            yield return new ValidationResult(
                "O campo DataLancamento e obrigatorio.",
                new[] { nameof(DataLancamento) });
        }
    }

    private static int ObterCasasDecimais(decimal valor)
    {
        var bits = decimal.GetBits(valor);
        return (bits[3] >> 16) & 0x7F;
    }
}
