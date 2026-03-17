using Lancamentos.Api;
using Lancamentos.Infraestrutura;

namespace Lancamentos.Testes.Integracao;

public sealed class EstruturaInicialTests
{
    [Fact]
    public void DeveReferenciarApiEInfraestrutura()
    {
        Assert.Equal("Lancamentos.Api", typeof(MarcadorApi).Assembly.GetName().Name);
        Assert.Equal("Lancamentos.Infraestrutura", typeof(MarcadorInfraestrutura).Assembly.GetName().Name);
    }
}
