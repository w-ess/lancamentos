using Lancamentos.Dominio.Entidades;
using Swashbuckle.AspNetCore.Annotations;

namespace Lancamentos.Aplicacao.Services;

/// <summary>
/// Dados retornados para um lancamento.
/// </summary>
public sealed record LancamentoDto(
    [property: SwaggerSchema("Identificador unico do lancamento.")]
    Guid Id,
    [property: SwaggerSchema("Tipo do lancamento: Credito ou Debito.")]
    string Tipo,
    [property: SwaggerSchema("Valor monetario do lancamento.")]
    decimal Valor,
    [property: SwaggerSchema("Data do lancamento no formato yyyy-MM-dd.", Format = "date")]
    DateOnly DataLancamento,
    [property: SwaggerSchema("Data e hora UTC em que o lancamento foi registrado.", Format = "date-time")]
    DateTime RegistradoEmUtc)
{
    public static LancamentoDto FromEntity(Lancamento lancamento)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        return new LancamentoDto(
            lancamento.Id,
            lancamento.Tipo.Valor,
            lancamento.Valor.Valor,
            lancamento.DataLancamento.Valor,
            lancamento.RegistradoEmUtc);
    }
}
