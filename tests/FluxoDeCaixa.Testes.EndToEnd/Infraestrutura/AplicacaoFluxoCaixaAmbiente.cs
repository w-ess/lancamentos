namespace FluxoDeCaixa.Testes.EndToEnd.Infraestrutura;

internal sealed class AplicacaoFluxoCaixaAmbiente : IAsyncDisposable
{
    private readonly List<ProcessoHospedado> _processos = [];
    private readonly CancellationTokenSource _cts = new();

    public AplicacaoFluxoCaixaAmbiente(
        string urlLancamentos,
        string? urlConsolidado)
    {
        UrlLancamentos = urlLancamentos;
        UrlConsolidado = urlConsolidado;
    }

    public string UrlLancamentos { get; }
    public string? UrlConsolidado { get; }

    public static async Task<AplicacaoFluxoCaixaAmbiente> IniciarAsync(
        InfraestruturaEndToEndFixture fixture,
        bool iniciarApiConsolidado,
        bool iniciarProcessador)
    {
        var diretorioRaiz = RepositorioRaizHelper.Obter();
        var portaLancamentos = PortaLivreHelper.ObterPortaLivreTcp();
        var portaConsolidado = iniciarApiConsolidado ? PortaLivreHelper.ObterPortaLivreTcp() : 0;

        var ambiente = new AplicacaoFluxoCaixaAmbiente(
            $"http://127.0.0.1:{portaLancamentos}",
            iniciarApiConsolidado ? $"http://127.0.0.1:{portaConsolidado}" : null);

        var variaveisBase = ambiente.CriarVariaveisBase(fixture);

        await ambiente.IniciarProcessoWebAsync(
            Path.Combine("src", "Lancamentos", "Lancamentos.Api", "Lancamentos.Api.csproj"),
            ambiente.UrlLancamentos,
            variaveisBase,
            diretorioRaiz);

        if (iniciarApiConsolidado)
        {
            await ambiente.IniciarProcessoWebAsync(
                Path.Combine("src", "ConsolidadoDiario", "ConsolidadoDiario.Api", "ConsolidadoDiario.Api.csproj"),
                ambiente.UrlConsolidado!,
                variaveisBase,
                diretorioRaiz);
        }

        if (iniciarProcessador)
        {
            await ambiente.IniciarProcessoWorkerAsync(
                Path.Combine("src", "ConsolidadoDiario", "ConsolidadoDiario.Processador", "ConsolidadoDiario.Processador.csproj"),
                variaveisBase,
                diretorioRaiz);
        }

        return ambiente;
    }

    public HttpClient CriarClienteAutenticado(InfraestruturaEndToEndFixture fixture, params string[] escopos)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(UrlLancamentos)
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", fixture.GerarToken(escopos));
        return client;
    }

    public HttpClient CriarClienteConsolidadoAutenticado(InfraestruturaEndToEndFixture fixture, params string[] escopos)
    {
        if (UrlConsolidado is null)
        {
            throw new InvalidOperationException("A API de ConsolidadoDiario nao foi iniciada neste ambiente.");
        }

        var client = new HttpClient
        {
            BaseAddress = new Uri(UrlConsolidado)
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", fixture.GerarToken(escopos));
        return client;
    }

    public async Task<T?> AguardarAteAsync<T>(
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

            await Task.Delay(250, _cts.Token);
        }

        return null;
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();

        for (var indice = _processos.Count - 1; indice >= 0; indice--)
        {
            await _processos[indice].DisposeAsync();
        }

        _cts.Dispose();
    }

    private Dictionary<string, string> CriarVariaveisBase(InfraestruturaEndToEndFixture fixture)
    {
        return new Dictionary<string, string>
        {
            ["DOTNET_ENVIRONMENT"] = "EndToEndTests",
            ["ASPNETCORE_ENVIRONMENT"] = "EndToEndTests",
            ["ConnectionStrings__LancamentosDb"] = fixture.ConnectionStringLancamentos,
            ["ConnectionStrings__ConsolidadoDiarioDb"] = fixture.ConnectionStringConsolidado,
            ["RabbitMq__Host"] = fixture.RabbitMqHost,
            ["RabbitMq__Port"] = fixture.RabbitMqPort.ToString(),
            ["RabbitMq__Usuario"] = "guest",
            ["RabbitMq__Senha"] = "guest",
            ["RabbitMq__VirtualHost"] = "/",
            ["PublicadorMensagensSaida__AtrasoInicialEmMilissegundos"] = "200",
            ["PublicadorMensagensSaida__IntervaloEmMilissegundos"] = "200",
            ["PublicadorMensagensSaida__QuantidadePorLote"] = "20",
            ["ConsultaSaldoDiario__AtrasoMaximoToleradoEmMinutos"] = "5",
            ["Autenticacao__Issuer"] = fixture.JwtIssuer,
            ["Autenticacao__Audience"] = fixture.JwtAudience,
            ["Autenticacao__ChaveAssinatura"] = fixture.JwtChaveAssinatura,
            ["Autenticacao__ExpiracaoEmMinutos"] = "60",
            ["Logging__LogLevel__Default"] = "Information",
            ["Logging__LogLevel__Microsoft"] = "Warning"
        };
    }

    private async Task IniciarProcessoWebAsync(
        string caminhoProjeto,
        string url,
        Dictionary<string, string> variaveisBase,
        string diretorioRaiz)
    {
        var processo = CriarProcesso(
            $"run --project \"{caminhoProjeto}\" -c Release --no-build --urls {url}",
            variaveisBase,
            diretorioRaiz);

        await processo.IniciarAsync(_cts.Token);
        _processos.Add(processo);
        await AguardarHealthAsync(url, processo, _cts.Token);
    }

    private async Task IniciarProcessoWorkerAsync(
        string caminhoProjeto,
        Dictionary<string, string> variaveisBase,
        string diretorioRaiz)
    {
        var processo = CriarProcesso(
            $"run --project \"{caminhoProjeto}\" -c Release --no-build",
            variaveisBase,
            diretorioRaiz);

        await processo.IniciarAsync(_cts.Token);
        _processos.Add(processo);
        await Task.Delay(1500, _cts.Token);

        if (processo.ExitCode is not null)
        {
            throw new InvalidOperationException(
                $"O processador encerrou prematuramente com codigo {processo.ExitCode}.{Environment.NewLine}{processo.ObterDiagnostico()}");
        }
    }

    private static ProcessoHospedado CriarProcesso(
        string argumentos,
        Dictionary<string, string> variaveisBase,
        string diretorioRaiz)
    {
        var startInfo = new ProcessStartInfo("dotnet", argumentos)
        {
            WorkingDirectory = diretorioRaiz,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var variavel in variaveisBase)
        {
            startInfo.Environment[variavel.Key] = variavel.Value;
        }

        return new ProcessoHospedado(new Process
        {
            StartInfo = startInfo
        });
    }

    private static async Task AguardarHealthAsync(
        string baseUrl,
        ProcessoHospedado processo,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(45))
        {
            if (processo.ExitCode is not null)
            {
                throw new InvalidOperationException(
                    $"O processo web encerrou com codigo {processo.ExitCode}.{Environment.NewLine}{processo.ObterDiagnostico()}");
            }

            try
            {
                var resposta = await client.GetAsync("/health", cancellationToken);

                if (resposta.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch
            {
            }

            await Task.Delay(250, cancellationToken);
        }

        throw new TimeoutException(
            $"Timeout aguardando health check em {baseUrl}.{Environment.NewLine}{processo.ObterDiagnostico()}");
    }
}
