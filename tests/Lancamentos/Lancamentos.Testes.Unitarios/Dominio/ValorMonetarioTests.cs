using Lancamentos.Dominio.Excecoes;
using Lancamentos.Dominio.ObjetosDeValor;

namespace Lancamentos.Testes.Unitarios.Dominio;

public sealed class ValorMonetarioTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(10.25)]
    public void DeveCriarValorPositivoComAteDuasCasasDecimais(decimal entrada)
    {
        var valor = ValorMonetario.Criar(entrada);

        Assert.Equal(entrada, valor.Valor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeveRejeitarValorMenorOuIgualAZero(decimal entrada)
    {
        var excecao = Assert.Throws<ExcecaoDominio>(() => ValorMonetario.Criar(entrada));

        Assert.NotEmpty(excecao.Message);
    }

    [Fact]
    public void DeveRejeitarValorComMaisDeDuasCasasDecimais()
    {
        var excecao = Assert.Throws<ExcecaoDominio>(() => ValorMonetario.Criar(10.257m));

        Assert.NotEmpty(excecao.Message);
    }
}
