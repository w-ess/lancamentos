namespace Processo.Lancamentos.Dominio.Excecoes;

public sealed class ExcecaoDominio : Exception
{
    public ExcecaoDominio(string message)
        : base(message)
    {
    }
}
