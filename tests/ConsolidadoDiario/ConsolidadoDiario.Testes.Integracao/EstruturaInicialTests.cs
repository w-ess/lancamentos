using ConsolidadoDiario.Api;
using ConsolidadoDiario.Infraestrutura;
using ConsolidadoDiario.Processador;

namespace ConsolidadoDiario.Testes.Integracao;

public sealed class EstruturaInicialTests
{
    [Fact]
    public void DeveReferenciarApiInfraestruturaEProcessador()
    {
        Assert.Equal("ConsolidadoDiario.Api", typeof(MarcadorApi).Assembly.GetName().Name);
        Assert.Equal("ConsolidadoDiario.Infraestrutura", typeof(MarcadorInfraestrutura).Assembly.GetName().Name);
        Assert.Equal("ConsolidadoDiario.Processador", typeof(MarcadorProcessador).Assembly.GetName().Name);
    }
}
