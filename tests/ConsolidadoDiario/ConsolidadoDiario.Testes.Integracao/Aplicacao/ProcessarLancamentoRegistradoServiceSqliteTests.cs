using ConsolidadoDiario.Aplicacao.Services.ProcessarLancamentoRegistrado;
using ConsolidadoDiario.Aplicacao.Integracao;
using ConsolidadoDiario.Infraestrutura.Repositorios;
using ConsolidadoDiario.Testes.Integracao.Infraestrutura;
using Microsoft.EntityFrameworkCore;

namespace ConsolidadoDiario.Testes.Integracao.Aplicacao;

public sealed class ProcessarLancamentoRegistradoServiceSqliteTests
{
    [Fact]
    public async Task DevePersistirSaldoDiarioELancamentoProcessadoNoBanco()
    {
        await using var factory = new ConsolidadoDiarioDbContextFactory();
        await factory.InicializarAsync();

        await using (var dbContext = factory.CriarDbContext())
        {
            var service = new ProcessarLancamentoRegistradoService(
                new ConsolidadoDiarioRepositorio(dbContext),
                new RelogioUtcFixo(new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc)));

            await service.ExecutarAsync(CriarEvento("Credito", 90m, Guid.NewGuid()));
        }

        await using (var dbContext = factory.CriarDbContext())
        {
            var saldo = await dbContext.SaldosDiarios.SingleAsync();
            var processado = await dbContext.LancamentosProcessados.SingleAsync();

            Assert.Equal(new DateOnly(2026, 3, 17), saldo.Data);
            Assert.Equal(90m, saldo.TotalCreditos);
            Assert.Equal(90m, saldo.Saldo);
            Assert.Equal("Credito", processado.Tipo.Valor);
            Assert.Equal(90m, processado.Valor.Valor);
        }
    }

    [Fact]
    public async Task DevePreservarIdempotenciaAoReprocessarMesmoLancamento()
    {
        var lancamentoId = Guid.NewGuid();

        await using var factory = new ConsolidadoDiarioDbContextFactory();
        await factory.InicializarAsync();

        await using (var dbContext = factory.CriarDbContext())
        {
            var service = new ProcessarLancamentoRegistradoService(
                new ConsolidadoDiarioRepositorio(dbContext),
                new RelogioUtcFixo(new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc)));

            await service.ExecutarAsync(CriarEvento("Credito", 90m, lancamentoId));
            await service.ExecutarAsync(CriarEvento("Credito", 90m, lancamentoId));
        }

        await using (var dbContext = factory.CriarDbContext())
        {
            var saldo = await dbContext.SaldosDiarios.SingleAsync();
            var quantidadeProcessados = await dbContext.LancamentosProcessados.CountAsync();

            Assert.Equal(90m, saldo.Saldo);
            Assert.Equal(1, quantidadeProcessados);
        }
    }

    private static LancamentoRegistradoV1 CriarEvento(string tipo, decimal valor, Guid lancamentoId)
    {
        return new LancamentoRegistradoV1(
            Guid.NewGuid(),
            new DateTime(2026, 3, 17, 15, 0, 0, DateTimeKind.Utc),
            lancamentoId,
            tipo,
            valor,
            new DateOnly(2026, 3, 17),
            "correlacao-sqlite");
    }
}
