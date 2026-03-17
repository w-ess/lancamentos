using Lancamentos.Testes.Integracao.Infraestrutura;
using Microsoft.AspNetCore.Mvc;

namespace Lancamentos.Testes.Integracao.Api;

public sealed class LancamentosEndpointsTests
{
    [Fact]
    public async Task DeveRegistrarELerLancamentoPelosEndpoints()
    {
        await using var factory = new LancamentosApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CreateClient();

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
        Assert.Equal(DateTimeKind.Utc, lancamentoCriado.RegistradoEmUtc.Kind);

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
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync($"/api/v1/lancamentos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoRequisicaoForInvalida()
    {
        await using var factory = new LancamentosApiFactory();
        await factory.InicializarBancoAsync();
        using var client = factory.CreateClient();

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
        using var client = factory.CreateClient();
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

    private sealed record LancamentoResponse(
        Guid Id,
        string Tipo,
        decimal Valor,
        DateOnly DataLancamento,
        DateTime RegistradoEmUtc);
}
