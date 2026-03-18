using ConsolidadoDiario.Aplicacao.Services.ProcessarLancamentoRegistrado;
using ConsolidadoDiario.Aplicacao.Integracao;
using ConsolidadoDiario.Dominio.Excecoes;
using ConsolidadoDiario.Testes.Unitarios.Doubles;

namespace ConsolidadoDiario.Testes.Unitarios.Aplicacao;

public sealed class ProcessarLancamentoRegistradoServiceTests
{
    [Fact]
    public async Task DeveProcessarCreditoERegistrarLancamentoProcessado()
    {
        var repositorio = new ConsolidadoDiarioRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var service = new ProcessarLancamentoRegistradoService(repositorio, relogio);

        var resultado = await service.ExecutarAsync(CriarEvento("Credito", 125.50m));

        var saldo = repositorio.ObterSaldo(new DateOnly(2026, 3, 17));
        var lancamentoProcessado = Assert.Single(repositorio.ListarLancamentosProcessados());

        Assert.False(resultado.JaProcessado);
        Assert.NotNull(saldo);
        Assert.Equal(125.50m, saldo.TotalCreditos);
        Assert.Equal(0m, saldo.TotalDebitos);
        Assert.Equal(125.50m, saldo.Saldo);
        Assert.Equal(relogio.UtcNow, saldo.Atualizado);
        Assert.Equal("correlacao-teste", lancamentoProcessado.CorrelacaoId);
    }

    [Fact]
    public async Task DeveIgnorarEventoDuplicadoPorLancamentoId()
    {
        var lancamentoId = Guid.NewGuid();
        var repositorio = new ConsolidadoDiarioRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var service = new ProcessarLancamentoRegistradoService(repositorio, relogio);
        var evento = CriarEvento("Credito", 80m, lancamentoId);

        await service.ExecutarAsync(evento);
        var resultadoDuplicado = await service.ExecutarAsync(evento with { EventoId = Guid.NewGuid() });

        var saldo = repositorio.ObterSaldo(new DateOnly(2026, 3, 17));

        Assert.True(resultadoDuplicado.JaProcessado);
        Assert.NotNull(saldo);
        Assert.Equal(80m, saldo.Saldo);
        Assert.Single(repositorio.ListarLancamentosProcessados());
    }

    [Fact]
    public async Task DeveSomarDebitoEmSaldoExistente()
    {
        var repositorio = new ConsolidadoDiarioRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var service = new ProcessarLancamentoRegistradoService(repositorio, relogio);

        await service.ExecutarAsync(CriarEvento("Credito", 200m, Guid.NewGuid()));
        await service.ExecutarAsync(CriarEvento("Debito", 45m, Guid.NewGuid()));

        var saldo = repositorio.ObterSaldo(new DateOnly(2026, 3, 17));

        Assert.NotNull(saldo);
        Assert.Equal(200m, saldo.TotalCreditos);
        Assert.Equal(45m, saldo.TotalDebitos);
        Assert.Equal(155m, saldo.Saldo);
    }

    [Fact]
    public async Task DevePropagarValidacaoDeDominioQuandoEventoForInvalido()
    {
        var repositorio = new ConsolidadoDiarioRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var service = new ProcessarLancamentoRegistradoService(repositorio, relogio);

        var excecao = await Assert.ThrowsAsync<ExcecaoDominio>(() =>
            service.ExecutarAsync(CriarEvento("Transferencia", 30m)));

        Assert.Equal("O tipo de lancamento deve ser Credito ou Debito.", excecao.Message);
    }

    private static LancamentoRegistradoV1 CriarEvento(string tipo, decimal valor, Guid? lancamentoId = null)
    {
        return new LancamentoRegistradoV1(
            Guid.NewGuid(),
            new DateTime(2026, 3, 17, 15, 0, 0, DateTimeKind.Utc),
            lancamentoId ?? Guid.NewGuid(),
            tipo,
            valor,
            new DateOnly(2026, 3, 17),
            "correlacao-teste");
    }
}
