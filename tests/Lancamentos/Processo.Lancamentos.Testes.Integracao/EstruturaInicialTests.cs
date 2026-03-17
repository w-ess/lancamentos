using Processo.Lancamentos.Api;
using Processo.Lancamentos.Infraestrutura;

namespace Processo.Lancamentos.Testes.Integracao;

public sealed class EstruturaInicialTests
{
    [Fact]
    public void DeveReferenciarApiEInfraestrutura()
    {
        Assert.Equal("Processo.Lancamentos.Api", typeof(MarcadorApi).Assembly.GetName().Name);
        Assert.Equal("Processo.Lancamentos.Infraestrutura", typeof(MarcadorInfraestrutura).Assembly.GetName().Name);
    }
}
