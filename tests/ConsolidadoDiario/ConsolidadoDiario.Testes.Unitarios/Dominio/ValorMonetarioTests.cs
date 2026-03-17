using ConsolidadoDiario.Dominio.Excecoes;
using ConsolidadoDiario.Dominio.ObjetosDeValor;

namespace ConsolidadoDiario.Testes.Unitarios.Dominio;

public sealed class ValorMonetarioTests
{
    [Fact]
    public void DeveCriarValorMonetarioValido()
    {
        var valor = ValorMonetario.Criar(123.45m);

        Assert.Equal(123.45m, valor.Valor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void DeveRejeitarValorNaoPositivo(decimal valorInvalido)
    {
        var excecao = Assert.Throws<ExcecaoDominio>(() => ValorMonetario.Criar(valorInvalido));

        Assert.Equal("O valor do lancamento deve ser maior que zero.", excecao.Message);
    }

    [Fact]
    public void DeveRejeitarMaisDeDuasCasasDecimais()
    {
        var excecao = Assert.Throws<ExcecaoDominio>(() => ValorMonetario.Criar(10.123m));

        Assert.Equal("O valor do lancamento deve ter no maximo duas casas decimais.", excecao.Message);
    }
}
