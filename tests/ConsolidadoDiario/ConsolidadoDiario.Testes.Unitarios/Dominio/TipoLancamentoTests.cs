using ConsolidadoDiario.Dominio.Excecoes;
using ConsolidadoDiario.Dominio.ObjetosDeValor;

namespace ConsolidadoDiario.Testes.Unitarios.Dominio;

public sealed class TipoLancamentoTests
{
    [Theory]
    [InlineData("Credito", true, false)]
    [InlineData("credito", true, false)]
    [InlineData("Debito", false, true)]
    [InlineData("debito", false, true)]
    public void DeveCriarTipoValido(string valor, bool esperadoCredito, bool esperadoDebito)
    {
        var tipo = TipoLancamento.Criar(valor);

        Assert.Equal(esperadoCredito, tipo.EhCredito);
        Assert.Equal(esperadoDebito, tipo.EhDebito);
    }

    [Fact]
    public void DeveRejeitarTipoInvalido()
    {
        var excecao = Assert.Throws<ExcecaoDominio>(() => TipoLancamento.Criar("Transferencia"));

        Assert.Equal("O tipo de lancamento deve ser Credito ou Debito.", excecao.Message);
    }
}
