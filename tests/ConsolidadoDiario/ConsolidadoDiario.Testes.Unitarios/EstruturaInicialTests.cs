using ConsolidadoDiario.Aplicacao;
using ConsolidadoDiario.Dominio;

namespace ConsolidadoDiario.Testes.Unitarios;

public sealed class EstruturaInicialTests
{
    [Fact]
    public void DeveReferenciarAsCamadasDeDominioEAplicacao()
    {
        Assert.Equal("ConsolidadoDiario.Dominio", typeof(MarcadorDominio).Assembly.GetName().Name);
        Assert.Equal("ConsolidadoDiario.Aplicacao", typeof(MarcadorAplicacao).Assembly.GetName().Name);
    }
}
