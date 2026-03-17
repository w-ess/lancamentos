using Processo.Lancamentos.Dominio.Excecoes;

namespace Processo.Lancamentos.Dominio.ObjetosDeValor;

public sealed record DataLancamento
{
    public DateOnly Valor { get; }

    private DataLancamento(DateOnly valor)
    {
        Valor = valor;
    }

    public static DataLancamento Criar(DateOnly valor)
    {
        if (valor == default)
        {
            throw new ExcecaoDominio("A data do lancamento e obrigatoria.");
        }

        return new DataLancamento(valor);
    }

    public override string ToString()
    {
        return Valor.ToString("yyyy-MM-dd");
    }
}
