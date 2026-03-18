using ConsolidadoDiario.Aplicacao.Services.ConsultarSaldoDiario;
using ConsolidadoDiario.Dominio.Entidades;
using ConsolidadoDiario.Dominio.ObjetosDeValor;
using ConsolidadoDiario.Testes.Unitarios.Doubles;

namespace ConsolidadoDiario.Testes.Unitarios.Aplicacao;

public sealed class ConsultarSaldoDiarioPorDataServiceTests
{
    [Fact]
    public async Task DeveRetornarSaldoExistenteSemDefasagemQuandoUltimoProcessamentoEstiverDentroDaTolerancia()
    {
        var dataConsulta = new DateOnly(2026, 3, 17);
        var atualizado = new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc);
        var repositorio = new ConsolidadoDiarioRepositorioEmMemoria();

        await repositorio.AdicionarSaldoAsync(SaldoDiario.Criar(dataConsulta, 150m, 40m, atualizado));
        await repositorio.AdicionarLancamentoProcessadoAsync(CriarLancamentoProcessado(dataConsulta, atualizado));

        var service = CreateService(
            repositorio,
            new DateTime(2026, 3, 17, 16, 4, 0, DateTimeKind.Utc),
            atrasoMaximoToleradoEmMinutos: 5);

        var resultado = await service.ExecutarAsync(dataConsulta);

        Assert.Equal(dataConsulta, resultado.Data);
        Assert.Equal(150m, resultado.TotalCreditos);
        Assert.Equal(40m, resultado.TotalDebitos);
        Assert.Equal(110m, resultado.Saldo);
        Assert.Equal(atualizado, resultado.Atualizado);
        Assert.False(resultado.Defasado);
    }

    [Fact]
    public async Task DeveRetornarSaldoZeradoParaDataSemMovimentoUsandoUltimaConfirmacaoConhecida()
    {
        var dataComMovimento = new DateOnly(2026, 3, 16);
        var dataSemMovimento = new DateOnly(2026, 3, 17);
        var ultimoProcessamentoUtc = new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc);
        var repositorio = new ConsolidadoDiarioRepositorioEmMemoria();

        await repositorio.AdicionarSaldoAsync(SaldoDiario.Criar(dataComMovimento, 90m, 10m, ultimoProcessamentoUtc));
        await repositorio.AdicionarLancamentoProcessadoAsync(CriarLancamentoProcessado(dataComMovimento, ultimoProcessamentoUtc));

        var service = CreateService(
            repositorio,
            new DateTime(2026, 3, 17, 16, 3, 0, DateTimeKind.Utc),
            atrasoMaximoToleradoEmMinutos: 5);

        var resultado = await service.ExecutarAsync(dataSemMovimento);

        Assert.Equal(dataSemMovimento, resultado.Data);
        Assert.Equal(0m, resultado.TotalCreditos);
        Assert.Equal(0m, resultado.TotalDebitos);
        Assert.Equal(0m, resultado.Saldo);
        Assert.Equal(ultimoProcessamentoUtc, resultado.Atualizado);
        Assert.False(resultado.Defasado);
    }

    [Fact]
    public async Task DeveMarcarComoDefasadoQuandoUltimoProcessamentoExcederTolerancia()
    {
        var dataConsulta = new DateOnly(2026, 3, 17);
        var atualizado = new DateTime(2026, 3, 17, 15, 50, 0, DateTimeKind.Utc);
        var repositorio = new ConsolidadoDiarioRepositorioEmMemoria();

        await repositorio.AdicionarSaldoAsync(SaldoDiario.Criar(dataConsulta, 75m, 25m, atualizado));
        await repositorio.AdicionarLancamentoProcessadoAsync(CriarLancamentoProcessado(dataConsulta, atualizado));

        var service = CreateService(
            repositorio,
            new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc),
            atrasoMaximoToleradoEmMinutos: 5);

        var resultado = await service.ExecutarAsync(dataConsulta);

        Assert.True(resultado.Defasado);
    }

    [Fact]
    public async Task DeveUsarHorarioAtualComoReferenciaQuandoAindaNaoHouverConfirmacaoConhecida()
    {
        var agoraUtc = new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc);
        var repositorio = new ConsolidadoDiarioRepositorioEmMemoria();
        var service = CreateService(repositorio, agoraUtc, atrasoMaximoToleradoEmMinutos: 5);

        var resultado = await service.ExecutarAsync(new DateOnly(2026, 3, 17));

        Assert.Equal(0m, resultado.Saldo);
        Assert.Equal(agoraUtc, resultado.Atualizado);
        Assert.False(resultado.Defasado);
    }

    private static ConsultarSaldoDiarioPorDataService CreateService(
        ConsolidadoDiarioRepositorioEmMemoria repositorio,
        DateTime agoraUtc,
        int atrasoMaximoToleradoEmMinutos)
    {
        return new ConsultarSaldoDiarioPorDataService(
            repositorio,
            new RelogioUtcFixo(agoraUtc),
            new ConsultarSaldoDiarioOpcoes
            {
                AtrasoMaximoToleradoEmMinutos = atrasoMaximoToleradoEmMinutos
            });
    }

    private static LancamentoProcessado CriarLancamentoProcessado(DateOnly dataLancamento, DateTime processado)
    {
        return LancamentoProcessado.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TipoLancamento.Credito,
            ValorMonetario.Criar(10m),
            dataLancamento,
            "correlacao-consulta",
            processado.AddMinutes(-1),
            processado);
    }
}
