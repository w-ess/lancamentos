using Processo.Lancamentos.Dominio.Excecoes;
using Processo.Lancamentos.Dominio.ObjetosDeValor;

namespace Processo.Lancamentos.Testes.Unitarios.Dominio;

public sealed class TipoLancamentoTests
{
    [Theory]
    [InlineData("Credito", true, false)]
    [InlineData("credito", true, false)]
    [InlineData(" Debito ", false, true)]
    public void DeveCriarTipoValido(string entrada, bool ehCredito, bool ehDebito)
    {
        var tipo = TipoLancamento.Criar(entrada);

        Assert.Equal(ehCredito, tipo.EhCredito);
        Assert.Equal(ehDebito, tipo.EhDebito);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Transferencia")]
    public void DeveRejeitarTipoInvalido(string entrada)
    {
        var excecao = Assert.Throws<ExcecaoDominio>(() => TipoLancamento.Criar(entrada));

        Assert.NotEmpty(excecao.Message);
    }
}
