using System.Net;
using System.Net.Http.Json;
using ConsolidadoDiario.Dominio.Entidades;
using ConsolidadoDiario.Dominio.ObjetosDeValor;
using ConsolidadoDiario.Testes.Integracao.Infraestrutura;
using Microsoft.AspNetCore.Mvc;

namespace ConsolidadoDiario.Testes.Integracao.Api;

public sealed class SaldosDiariosEndpointsTests
{
    [Fact]
    public async Task DeveRetornarSaldoDiarioParaDataComMovimento()
    {
        var atualizadoEmUtc = new DateTime(2026, 3, 17, 15, 58, 0, DateTimeKind.Utc);

        await using var factory = new ConsolidadoDiarioApiFactory(
            agoraUtc: new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc));
        await factory.InicializarBancoAsync();
        await factory.ExecutarNoDbContextAsync(async dbContext =>
        {
            await dbContext.SaldosDiarios.AddAsync(SaldoDiario.Criar(new DateOnly(2026, 3, 17), 120.50m, 20m, atualizadoEmUtc));
            await dbContext.LancamentosProcessados.AddAsync(CriarLancamentoProcessado(
                new DateOnly(2026, 3, 17),
                "Credito",
                120.50m,
                atualizadoEmUtc));
            await dbContext.SaveChangesAsync();
        });

        using var client = factory.CriarClientAutenticado("consolidado.leitura");

        var resposta = await client.GetAsync("/api/v1/saldos-diarios/2026-03-17");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var saldo = await resposta.Content.ReadFromJsonAsync<SaldoDiarioResponse>();

        Assert.NotNull(saldo);
        Assert.Equal(new DateOnly(2026, 3, 17), saldo.Data);
        Assert.Equal(120.50m, saldo.TotalCreditos);
        Assert.Equal(20m, saldo.TotalDebitos);
        Assert.Equal(100.50m, saldo.Saldo);
        Assert.Equal(atualizadoEmUtc, saldo.AtualizadoEmUtc);
        Assert.False(saldo.Defasado);
    }

    [Fact]
    public async Task DeveRetornarZerosParaDataSemMovimentoUsandoUltimaConfirmacaoConhecida()
    {
        var ultimoProcessamentoUtc = new DateTime(2026, 3, 17, 15, 59, 0, DateTimeKind.Utc);

        await using var factory = new ConsolidadoDiarioApiFactory(
            agoraUtc: new DateTime(2026, 3, 17, 16, 1, 0, DateTimeKind.Utc));
        await factory.InicializarBancoAsync();
        await factory.ExecutarNoDbContextAsync(async dbContext =>
        {
            await dbContext.SaldosDiarios.AddAsync(SaldoDiario.Criar(new DateOnly(2026, 3, 16), 80m, 10m, ultimoProcessamentoUtc));
            await dbContext.LancamentosProcessados.AddAsync(CriarLancamentoProcessado(
                new DateOnly(2026, 3, 16),
                "Credito",
                80m,
                ultimoProcessamentoUtc));
            await dbContext.SaveChangesAsync();
        });

        using var client = factory.CriarClientAutenticado("consolidado.leitura");

        var resposta = await client.GetAsync("/api/v1/saldos-diarios/2026-03-17");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var saldo = await resposta.Content.ReadFromJsonAsync<SaldoDiarioResponse>();

        Assert.NotNull(saldo);
        Assert.Equal(new DateOnly(2026, 3, 17), saldo.Data);
        Assert.Equal(0m, saldo.TotalCreditos);
        Assert.Equal(0m, saldo.TotalDebitos);
        Assert.Equal(0m, saldo.Saldo);
        Assert.Equal(ultimoProcessamentoUtc, saldo.AtualizadoEmUtc);
        Assert.False(saldo.Defasado);
    }

    [Fact]
    public async Task DeveIndicarQuandoSaldoEstiverDefasado()
    {
        var atualizadoEmUtc = new DateTime(2026, 3, 17, 15, 50, 0, DateTimeKind.Utc);

        await using var factory = new ConsolidadoDiarioApiFactory(
            agoraUtc: new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc),
            atrasoMaximoToleradoEmMinutos: 5);
        await factory.InicializarBancoAsync();
        await factory.ExecutarNoDbContextAsync(async dbContext =>
        {
            await dbContext.SaldosDiarios.AddAsync(SaldoDiario.Criar(new DateOnly(2026, 3, 17), 60m, 10m, atualizadoEmUtc));
            await dbContext.LancamentosProcessados.AddAsync(CriarLancamentoProcessado(
                new DateOnly(2026, 3, 17),
                "Credito",
                60m,
                atualizadoEmUtc));
            await dbContext.SaveChangesAsync();
        });

        using var client = factory.CriarClientAutenticado("consolidado.leitura");

        var resposta = await client.GetAsync("/api/v1/saldos-diarios/2026-03-17");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var saldo = await resposta.Content.ReadFromJsonAsync<SaldoDiarioResponse>();

        Assert.NotNull(saldo);
        Assert.True(saldo.Defasado);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoDataForInvalida()
    {
        await using var factory = new ConsolidadoDiarioApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CriarClientAutenticado("consolidado.leitura");

        var resposta = await client.GetAsync("/api/v1/saldos-diarios/17-03-2026");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var problemDetails = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
        Assert.Contains("Data", problemDetails.Errors.Keys);
    }

    [Fact]
    public async Task DeveRetornarUnauthorizedQuandoTokenNaoForInformado()
    {
        await using var factory = new ConsolidadoDiarioApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/v1/saldos-diarios/2026-03-17");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarForbiddenQuandoTokenNaoPossuirPermissaoDeLeituraDoConsolidado()
    {
        await using var factory = new ConsolidadoDiarioApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CriarClientAutenticado("lancamentos.leitura");

        var resposta = await client.GetAsync("/api/v1/saldos-diarios/2026-03-17");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    private static LancamentoProcessado CriarLancamentoProcessado(
        DateOnly dataLancamento,
        string tipo,
        decimal valor,
        DateTime processadoEmUtc)
    {
        return LancamentoProcessado.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TipoLancamento.Criar(tipo),
            ValorMonetario.Criar(valor),
            dataLancamento,
            "correlacao-api",
            processadoEmUtc.AddMinutes(-1),
            processadoEmUtc);
    }

    private sealed record SaldoDiarioResponse(
        DateOnly Data,
        decimal TotalCreditos,
        decimal TotalDebitos,
        decimal Saldo,
        DateTime AtualizadoEmUtc,
        bool Defasado);
}
