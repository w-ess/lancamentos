using Lancamentos.Dominio.Excecoes;

namespace Lancamentos.Dominio.ObjetosDeValor;

public sealed record ValorMonetario
{
    public decimal Valor { get; }

    private ValorMonetario(decimal valor)
    {
        Valor = valor;
    }

    public static ValorMonetario Criar(decimal valor)
    {
        if (valor <= 0)
        {
            throw new ExcecaoDominio("O valor do lancamento deve ser maior que zero.");
        }

        if (ObterCasasDecimais(valor) > 2)
        {
            throw new ExcecaoDominio("O valor do lancamento deve ter no maximo duas casas decimais.");
        }

        return new ValorMonetario(valor);
    }

    public override string ToString()
    {
        return Valor.ToString("0.00");
    }

    private static int ObterCasasDecimais(decimal valor)
    {
        var bits = decimal.GetBits(valor);
        return (bits[3] >> 16) & 0x7F;
    }
}
