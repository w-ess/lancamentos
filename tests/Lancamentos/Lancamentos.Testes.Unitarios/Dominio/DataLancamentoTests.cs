using Lancamentos.Dominio.Excecoes;
using Lancamentos.Dominio.ObjetosDeValor;

namespace Lancamentos.Testes.Unitarios.Dominio;

public sealed class DataLancamentoTests
{
    [Fact]
    public void DeveCriarDataValida()
    {
        var data = DataLancamento.Criar(new DateOnly(2026, 3, 17));

        Assert.Equal(new DateOnly(2026, 3, 17), data.Valor);
    }

    [Fact]
    public void DeveRejeitarDataPadrao()
    {
        var excecao = Assert.Throws<ExcecaoDominio>(() => DataLancamento.Criar(default));

        Assert.NotEmpty(excecao.Message);
    }
}
