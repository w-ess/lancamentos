using Processo.ConsolidadoDiario.Aplicacao;
using Processo.ConsolidadoDiario.Dominio;

namespace Processo.ConsolidadoDiario.Testes.Unitarios;

public sealed class EstruturaInicialTests
{
    [Fact]
    public void DeveReferenciarAsCamadasDeDominioEAplicacao()
    {
        Assert.Equal("Processo.ConsolidadoDiario.Dominio", typeof(MarcadorDominio).Assembly.GetName().Name);
        Assert.Equal("Processo.ConsolidadoDiario.Aplicacao", typeof(MarcadorAplicacao).Assembly.GetName().Name);
    }
}
