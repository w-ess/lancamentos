using ConsolidadoDiario.Dominio.Entidades;
using ConsolidadoDiario.Dominio.Excecoes;
using ConsolidadoDiario.Dominio.ObjetosDeValor;

namespace ConsolidadoDiario.Testes.Unitarios.Dominio;

public sealed class LancamentoProcessadoTests
{
    [Fact]
    public void DeveCriarLancamentoProcessadoValido()
    {
        var lancamento = LancamentoProcessado.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TipoLancamento.Criar("Credito"),
            ValorMonetario.Criar(10m),
            new DateOnly(2026, 3, 17),
            "correlacao-1",
            new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 17, 12, 1, 0, DateTimeKind.Utc));

        Assert.Equal("correlacao-1", lancamento.CorrelacaoId);
        Assert.Equal(new DateOnly(2026, 3, 17), lancamento.DataLancamento);
    }

    [Fact]
    public void DeveRejeitarCorrelacaoVazia()
    {
        var excecao = Assert.Throws<ExcecaoDominio>(() => LancamentoProcessado.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TipoLancamento.Criar("Credito"),
            ValorMonetario.Criar(10m),
            new DateOnly(2026, 3, 17),
            "",
            new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 17, 12, 1, 0, DateTimeKind.Utc)));

        Assert.Equal("O identificador de correlacao do lancamento processado e obrigatorio.", excecao.Message);
    }
}
