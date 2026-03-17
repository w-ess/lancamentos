using System.ComponentModel.DataAnnotations;

namespace Lancamentos.Infraestrutura.Mensageria;

public sealed class PublicadorMensagensSaidaOpcoes
{
    public const string Secao = "PublicadorMensagensSaida";

    [Range(0, int.MaxValue)]
    public int AtrasoInicialEmMilissegundos { get; init; }

    [Range(1, int.MaxValue)]
    public int IntervaloEmMilissegundos { get; init; } = 2000;

    [Range(1, 1000)]
    public int QuantidadePorLote { get; init; } = 20;
}
