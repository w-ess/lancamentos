namespace ConsolidadoDiario.Aplicacao.Services.ConsultarSaldoDiario;

public sealed class ConsultarSaldoDiarioOpcoes
{
    public const string Secao = "ConsultaSaldoDiario";

    public int AtrasoMaximoToleradoEmMinutos { get; init; } = 5;

    public TimeSpan ObterToleranciaDefasagem()
    {
        if (AtrasoMaximoToleradoEmMinutos <= 0)
        {
            throw new InvalidOperationException("A configuracao de consulta do saldo diario exige um atraso maximo tolerado maior que zero.");
        }

        return TimeSpan.FromMinutes(AtrasoMaximoToleradoEmMinutos);
    }
}
