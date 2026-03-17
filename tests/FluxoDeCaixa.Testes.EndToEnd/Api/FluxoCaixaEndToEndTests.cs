using FluxoDeCaixa.Testes.EndToEnd.Infraestrutura;

namespace FluxoDeCaixa.Testes.EndToEnd.Api;

[Collection(InfraestruturaEndToEndCollection.Nome)]
public sealed class FluxoCaixaEndToEndTests
{
    private readonly InfraestruturaEndToEndFixture _fixture;

    public FluxoCaixaEndToEndTests(InfraestruturaEndToEndFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DeveCriarLancamentoEAtualizarConsolidadoEmFluxoCompleto()
    {
        await using var ambiente = await AplicacaoFluxoCaixaAmbiente.IniciarAsync(
            _fixture,
            iniciarApiConsolidado: true,
            iniciarProcessador: true);

        using var clientLancamentos = ambiente.CriarClienteAutenticado(
            _fixture,
            "lancamentos.escrita",
            "lancamentos.leitura");
        using var clientConsolidado = ambiente.CriarClienteConsolidadoAutenticado(
            _fixture,
            "consolidado.leitura");

        var requisicao = new
        {
            Tipo = "Credito",
            Valor = 150.75m,
            DataLancamento = new DateOnly(2026, 3, 17)
        };

        var respostaPost = await clientLancamentos.PostAsJsonAsync("/api/v1/lancamentos", requisicao);

        Assert.Equal(HttpStatusCode.Created, respostaPost.StatusCode);

        var saldoAtualizado = await ambiente.AguardarAteAsync(
            async () =>
            {
                var resposta = await clientConsolidado.GetAsync("/api/v1/saldos-diarios/2026-03-17");

                if (resposta.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var saldo = await resposta.Content.ReadFromJsonAsync<SaldoDiarioResponse>();

                return saldo is { TotalCreditos: 150.75m, Saldo: 150.75m, Defasado: false }
                    ? saldo
                    : null;
            },
            TimeSpan.FromSeconds(90));

        Assert.True(
            saldoAtualizado is not null,
            $"O consolidado nao foi atualizado dentro do tempo esperado.{Environment.NewLine}{ambiente.ObterDiagnostico()}");
        Assert.Equal(new DateOnly(2026, 3, 17), saldoAtualizado.Data);
        Assert.Equal(0m, saldoAtualizado.TotalDebitos);
    }

    [Fact]
    public async Task DeveExigirAutenticacaoEPermissoesNosEndpointsProtegidos()
    {
        await using var ambiente = await AplicacaoFluxoCaixaAmbiente.IniciarAsync(
            _fixture,
            iniciarApiConsolidado: true,
            iniciarProcessador: false);

        using var clientAnonimo = new HttpClient
        {
            BaseAddress = new Uri(ambiente.UrlLancamentos)
        };
        using var clientSemEscopoConsolidado = ambiente.CriarClienteConsolidadoAutenticado(
            _fixture,
            "lancamentos.leitura");

        var respostaSemToken = await clientAnonimo.GetAsync($"/api/v1/lancamentos/{Guid.NewGuid()}");
        var respostaSemPermissao = await clientSemEscopoConsolidado.GetAsync("/api/v1/saldos-diarios/2026-03-17");

        Assert.Equal(HttpStatusCode.Unauthorized, respostaSemToken.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, respostaSemPermissao.StatusCode);
    }

    [Fact]
    public async Task DevePersistirLancamentoMesmoSemConsolidadoDisponivel()
    {
        await using var ambiente = await AplicacaoFluxoCaixaAmbiente.IniciarAsync(
            _fixture,
            iniciarApiConsolidado: false,
            iniciarProcessador: false);

        using var clientLancamentos = ambiente.CriarClienteAutenticado(
            _fixture,
            "lancamentos.escrita",
            "lancamentos.leitura");

        var requisicao = new
        {
            Tipo = "Debito",
            Valor = 42.10m,
            DataLancamento = new DateOnly(2026, 3, 17)
        };

        var respostaPost = await clientLancamentos.PostAsJsonAsync("/api/v1/lancamentos", requisicao);
        var lancamento = await respostaPost.Content.ReadFromJsonAsync<LancamentoResponse>();

        Assert.Equal(HttpStatusCode.Created, respostaPost.StatusCode);
        Assert.NotNull(lancamento);

        await Task.Delay(1500);

        var respostaGet = await clientLancamentos.GetAsync($"/api/v1/lancamentos/{lancamento.Id}");

        Assert.Equal(HttpStatusCode.OK, respostaGet.StatusCode);

        var lancamentoPersistido = await respostaGet.Content.ReadFromJsonAsync<LancamentoResponse>();

        Assert.NotNull(lancamentoPersistido);
        Assert.Equal(lancamento.Id, lancamentoPersistido.Id);
        Assert.Equal("Debito", lancamentoPersistido.Tipo);
        Assert.Equal(42.10m, lancamentoPersistido.Valor);
    }

    private sealed record LancamentoResponse(
        Guid Id,
        string Tipo,
        decimal Valor,
        DateOnly DataLancamento,
        DateTime RegistradoEmUtc);

    private sealed record SaldoDiarioResponse(
        DateOnly Data,
        decimal TotalCreditos,
        decimal TotalDebitos,
        decimal Saldo,
        DateTime AtualizadoEmUtc,
        bool Defasado);
}
