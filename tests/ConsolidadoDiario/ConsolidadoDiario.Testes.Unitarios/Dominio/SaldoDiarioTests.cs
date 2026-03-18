using ConsolidadoDiario.Dominio.Entidades;
using ConsolidadoDiario.Dominio.Excecoes;
using ConsolidadoDiario.Dominio.ObjetosDeValor;

namespace ConsolidadoDiario.Testes.Unitarios.Dominio;

public sealed class SaldoDiarioTests
{
    [Fact]
    public void DeveAplicarCreditoEAtualizarSaldo()
    {
        var saldo = SaldoDiario.Criar(
            new DateOnly(2026, 3, 17),
            new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc));

        saldo.AplicarLancamento(
            TipoLancamento.Criar("Credito"),
            ValorMonetario.Criar(150.75m),
            new DateTime(2026, 3, 17, 12, 5, 0, DateTimeKind.Utc));

        Assert.Equal(150.75m, saldo.TotalCreditos);
        Assert.Equal(0m, saldo.TotalDebitos);
        Assert.Equal(150.75m, saldo.Saldo);
        Assert.Equal(new DateTime(2026, 3, 17, 12, 5, 0, DateTimeKind.Utc), saldo.Atualizado);
    }

    [Fact]
    public void DeveAplicarDebitoEAtualizarSaldo()
    {
        var saldo = SaldoDiario.Criar(
            new DateOnly(2026, 3, 17),
            totalCreditos: 200m,
            totalDebitos: 50m,
            atualizado: new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc));

        saldo.AplicarLancamento(
            TipoLancamento.Criar("Debito"),
            ValorMonetario.Criar(25m),
            new DateTime(2026, 3, 17, 12, 10, 0, DateTimeKind.Utc));

        Assert.Equal(200m, saldo.TotalCreditos);
        Assert.Equal(75m, saldo.TotalDebitos);
        Assert.Equal(125m, saldo.Saldo);
    }

    [Fact]
    public void DeveRejeitarAtualizacaoForaDeUtc()
    {
        var saldo = SaldoDiario.Criar(
            new DateOnly(2026, 3, 17),
            new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc));

        var excecao = Assert.Throws<ExcecaoDominio>(() => saldo.AplicarLancamento(
            TipoLancamento.Criar("Credito"),
            ValorMonetario.Criar(10m),
            new DateTime(2026, 3, 17, 9, 0, 0, DateTimeKind.Local)));

        Assert.Equal("A data de atualizacao do saldo diario deve estar em UTC.", excecao.Message);
    }
}
