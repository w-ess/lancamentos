using System.ComponentModel.DataAnnotations;

namespace ConsolidadoDiario.Processador.Mensageria;

public sealed class RabbitMqOpcoes
{
    public const string Secao = "RabbitMq";

    [Required]
    public string Host { get; init; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; init; } = 5672;

    [Required]
    public string Usuario { get; init; } = "guest";

    [Required]
    public string Senha { get; init; } = "guest";

    [Required]
    public string VirtualHost { get; init; } = "/";
}
