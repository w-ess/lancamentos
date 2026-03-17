using Swashbuckle.AspNetCore.Annotations;

namespace ConsolidadoDiario.Aplicacao.Services;

/// <summary>
/// Dados do saldo diario consolidado.
/// </summary>
public sealed record SaldoDiarioDto(
    [property: SwaggerSchema("Data de referencia do saldo.", Format = "date")]
    DateOnly Data,
    [property: SwaggerSchema("Soma de todos os creditos da data.")]
    decimal TotalCreditos,
    [property: SwaggerSchema("Soma de todos os debitos da data.")]
    decimal TotalDebitos,
    [property: SwaggerSchema("Saldo final da data.")]
    decimal Saldo,
    [property: SwaggerSchema("Data e hora UTC da ultima atualizacao do consolidado.", Format = "date-time")]
    DateTime AtualizadoEmUtc,
    [property: SwaggerSchema("Indica se o saldo ainda pode estar defasado em relacao aos lancamentos mais recentes.")]
    bool Defasado);
