using System.Diagnostics;
using System.Text.Json;
using Lancamentos.Aplicacao.Integracao;
using Lancamentos.Infraestrutura.Persistencia;
using Lancamentos.Testes.Integracao.Infraestrutura;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Lancamentos.Testes.Integracao.Api;

public sealed class LancamentosEndpointsTests
{
    [Fact]
    public async Task DeveRegistrarELerLancamentoPelosEndpoints()
    {
        await using var factory = new LancamentosApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CriarClientAutenticado("lancamentos.escrita", "lancamentos.leitura");

        var requisicao = new
        {
            Tipo = "Credito",
            Valor = 150.75m,
            DataLancamento = new DateOnly(2026, 3, 17)
        };

        var respostaPost = await client.PostAsJsonAsync("/api/v1/lancamentos", requisicao);

        Assert.Equal(HttpStatusCode.Created, respostaPost.StatusCode);
        Assert.NotNull(respostaPost.Headers.Location);

        var lancamentoCriado = await respostaPost.Content.ReadFromJsonAsync<LancamentoResponse>();

        Assert.NotNull(lancamentoCriado);
        Assert.NotEqual(Guid.Empty, lancamentoCriado.Id);
        Assert.Equal("Credito", lancamentoCriado.Tipo);
        Assert.Equal(150.75m, lancamentoCriado.Valor);
        Assert.Equal(new DateOnly(2026, 3, 17), lancamentoCriado.DataLancamento);
        Assert.Equal(DateTimeKind.Utc, lancamentoCriado.Registrado.Kind);

        var respostaGet = await client.GetAsync($"/api/v1/lancamentos/{lancamentoCriado.Id}");

        Assert.Equal(HttpStatusCode.OK, respostaGet.StatusCode);

        var lancamentoConsultado = await respostaGet.Content.ReadFromJsonAsync<LancamentoResponse>();

        Assert.NotNull(lancamentoConsultado);
        Assert.Equal(lancamentoCriado.Id, lancamentoConsultado.Id);
        Assert.Equal(lancamentoCriado.Tipo, lancamentoConsultado.Tipo);
        Assert.Equal(lancamentoCriado.Valor, lancamentoConsultado.Valor);
        Assert.Equal(lancamentoCriado.DataLancamento, lancamentoConsultado.DataLancamento);
    }

    [Fact]
    public async Task DeveRetornarNotFoundQuandoLancamentoNaoExistir()
    {
        await using var factory = new LancamentosApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CriarClientAutenticado("lancamentos.leitura");

        var resposta = await client.GetAsync($"/api/v1/lancamentos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoRequisicaoForInvalida()
    {
        await using var factory = new LancamentosApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CriarClientAutenticado("lancamentos.escrita");

        var resposta = await client.PostAsJsonAsync("/api/v1/lancamentos", new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var problemDetails = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
        Assert.Contains("Tipo", problemDetails.Errors.Keys);
        Assert.Contains("Valor", problemDetails.Errors.Keys);
        Assert.Contains("DataLancamento", problemDetails.Errors.Keys);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoCorpoJsonForInvalido()
    {
        await using var factory = new LancamentosApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CriarClientAutenticado("lancamentos.escrita");
        using var content = new StringContent(
            "{\"Tipo\":\"Credito\",\"Valor\":10.00,\"DataLancamento\":\"17/03/2026\"}",
            System.Text.Encoding.UTF8,
            "application/json");

        var resposta = await client.PostAsync("/api/v1/lancamentos", content);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var problemDetails = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal("Requisicao invalida.", problemDetails.Title);
        Assert.Equal("O corpo da requisicao esta mal formatado.", problemDetails.Detail);
    }

    [Fact]
    public async Task DevePersistirEPublicarOutboxMessageEmBackground()
    {
        await using var factory = new LancamentosApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CriarClientAutenticado("lancamentos.escrita");

        const string correlacaoId = "corr-integracao-publicacao";
        var requisicao = new
        {
            Tipo = "Debito",
            Valor = 50.25m,
            DataLancamento = new DateOnly(2026, 3, 17)
        };

        using var mensagem = new HttpRequestMessage(HttpMethod.Post, "/api/v1/lancamentos")
        {
            Content = JsonContent.Create(requisicao)
        };
        mensagem.Headers.Add("X-Correlation-Id", correlacaoId);

        var resposta = await client.SendAsync(mensagem);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var mensagemSaida = await AguardarAteAsync(
            async () => await factory.ExecutarNoDbContextAsync(async dbContext =>
                await dbContext.OutboxMessages
                    .AsNoTracking()
                    .SingleOrDefaultAsync(item =>
                        item.CorrelacaoId == correlacaoId &&
                        item.Publicada != null)),
            TimeSpan.FromSeconds(5));

        Assert.NotNull(mensagemSaida);
        Assert.NotNull(mensagemSaida.Publicada);
        Assert.Equal(0, mensagemSaida.TentativasPublicacao);
        Assert.Null(mensagemSaida.UltimoErro);

        var evento = JsonSerializer.Deserialize<LancamentoRegistradoV1>(mensagemSaida.Conteudo);

        Assert.NotNull(evento);
        Assert.Equal(correlacaoId, evento.CorrelacaoId);
        Assert.Equal(mensagemSaida.Id, evento.EventoId);
        Assert.Equal("Debito", evento.Tipo);
        Assert.Single(factory.PublicadorMensagens.ListarMensagensPublicadas());
    }

    [Fact]
    public async Task DevePersistirLancamentoMesmoQuandoPublicacaoFalhar()
    {
        await using var factory = new LancamentosApiFactory(falhasRestantesPublicacao: int.MaxValue);
        await factory.InicializarBancoAsync();
        using var client = factory.CriarClientAutenticado("lancamentos.escrita", "lancamentos.leitura");

        const string correlacaoId = "corr-integracao-falha";
        var requisicao = new
        {
            Tipo = "Credito",
            Valor = 10.00m,
            DataLancamento = new DateOnly(2026, 3, 17)
        };

        using var mensagem = new HttpRequestMessage(HttpMethod.Post, "/api/v1/lancamentos")
        {
            Content = JsonContent.Create(requisicao)
        };
        mensagem.Headers.Add("X-Correlation-Id", correlacaoId);

        var respostaPost = await client.SendAsync(mensagem);
        var lancamentoCriado = await respostaPost.Content.ReadFromJsonAsync<LancamentoResponse>();

        Assert.Equal(HttpStatusCode.Created, respostaPost.StatusCode);
        Assert.NotNull(lancamentoCriado);

        var respostaGet = await client.GetAsync($"/api/v1/lancamentos/{lancamentoCriado.Id}");

        Assert.Equal(HttpStatusCode.OK, respostaGet.StatusCode);

        var mensagemSaida = await AguardarAteAsync(
            async () => await factory.ExecutarNoDbContextAsync(async dbContext =>
                await dbContext.OutboxMessages
                    .AsNoTracking()
                    .SingleOrDefaultAsync(item =>
                        item.CorrelacaoId == correlacaoId &&
                        item.TentativasPublicacao > 0)),
            TimeSpan.FromSeconds(5));

        Assert.NotNull(mensagemSaida);
        Assert.Null(mensagemSaida.Publicada);
        Assert.NotNull(mensagemSaida.UltimoErro);
        Assert.Empty(factory.PublicadorMensagens.ListarMensagensPublicadas());
    }

    [Fact]
    public async Task DeveRetornarUnauthorizedQuandoTokenNaoForInformado()
    {
        await using var factory = new LancamentosApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync($"/api/v1/lancamentos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarForbiddenQuandoTokenNaoPossuirPermissaoDeEscrita()
    {
        await using var factory = new LancamentosApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CriarClientAutenticado("lancamentos.leitura");

        var requisicao = new
        {
            Tipo = "Credito",
            Valor = 15m,
            DataLancamento = new DateOnly(2026, 3, 17)
        };

        var resposta = await client.PostAsJsonAsync("/api/v1/lancamentos", requisicao);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    private static async Task<T?> AguardarAteAsync<T>(
        Func<Task<T?>> obterValorAsync,
        TimeSpan timeout)
        where T : class
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            var valor = await obterValorAsync();

            if (valor is not null)
            {
                return valor;
            }

            await Task.Delay(100);
        }

        return null;
    }

    private sealed record LancamentoResponse(
        Guid Id,
        string Tipo,
        decimal Valor,
        DateOnly DataLancamento,
        DateTime Registrado);
}
