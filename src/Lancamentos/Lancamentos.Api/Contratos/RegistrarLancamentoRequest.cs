using System.ComponentModel.DataAnnotations;
using Lancamentos.Aplicacao.CasosDeUso.RegistrarLancamento;

namespace Lancamentos.Api.Contratos;

public sealed class RegistrarLancamentoRequest : IValidatableObject
{
    public string? Tipo { get; init; }

    public decimal? Valor { get; init; }

    public DateOnly? DataLancamento { get; init; }

    public RegistrarLancamentoComando ParaComando(string correlacaoId)
    {
        return new RegistrarLancamentoComando(
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
