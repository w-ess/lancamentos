using Processo.Lancamentos.Dominio.Entidades;
using Processo.Lancamentos.Dominio.Excecoes;
using Processo.Lancamentos.Dominio.ObjetosDeValor;

namespace Processo.Lancamentos.Testes.Unitarios.Dominio;

public sealed class LancamentoTests
{
    [Fact]
    public void DeveCriarLancamentoValido()
    {
        var tipo = TipoLancamento.Criar("Credito");
        var valor = ValorMonetario.Criar(150.75m);
        var data = DataLancamento.Criar(new DateOnly(2026, 3, 17));
        var registradoEmUtc = new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc);

        var lancamento = Lancamento.Criar(tipo, valor, data, registradoEmUtc);

        Assert.NotEqual(Guid.Empty, lancamento.Id);
        Assert.Equal(tipo, lancamento.Tipo);
        Assert.Equal(valor, lancamento.Valor);
        Assert.Equal(data, lancamento.DataLancamento);
        Assert.Equal(registradoEmUtc, lancamento.RegistradoEmUtc);
    }

    [Fact]
    public void DeveRejeitarRegistroQueNaoEstaEmUtc()
    {
        var tipo = TipoLancamento.Criar("Debito");
        var valor = ValorMonetario.Criar(20m);
        var data = DataLancamento.Criar(new DateOnly(2026, 3, 17));
        var registradoEmLocal = new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Local);

        var excecao = Assert.Throws<ExcecaoDominio>(() =>
            Lancamento.Criar(tipo, valor, data, registradoEmLocal));

        Assert.NotEmpty(excecao.Message);
    }

    [Fact]
    public void DeveRejeitarIdentificadorVazio()
    {
        var tipo = TipoLancamento.Criar("Credito");
        var valor = ValorMonetario.Criar(80m);
        var data = DataLancamento.Criar(new DateOnly(2026, 3, 17));
        var registradoEmUtc = new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc);

        var excecao = Assert.Throws<ExcecaoDominio>(() =>
            Lancamento.Criar(Guid.Empty, tipo, valor, data, registradoEmUtc));

        Assert.NotEmpty(excecao.Message);
    }
}
