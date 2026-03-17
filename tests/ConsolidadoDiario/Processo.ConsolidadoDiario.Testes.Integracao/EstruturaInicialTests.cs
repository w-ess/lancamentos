using Processo.ConsolidadoDiario.Api;
using Processo.ConsolidadoDiario.Infraestrutura;
using Processo.ConsolidadoDiario.Processador;

namespace Processo.ConsolidadoDiario.Testes.Integracao;

public sealed class EstruturaInicialTests
{
    [Fact]
    public void DeveReferenciarApiInfraestruturaEProcessador()
    {
        Assert.Equal("Processo.ConsolidadoDiario.Api", typeof(MarcadorApi).Assembly.GetName().Name);
        Assert.Equal("Processo.ConsolidadoDiario.Infraestrutura", typeof(MarcadorInfraestrutura).Assembly.GetName().Name);
        Assert.Equal("Processo.ConsolidadoDiario.Processador", typeof(MarcadorProcessador).Assembly.GetName().Name);
    }
}
