using System.Globalization;
using System.Net.Http.Headers;
using ConsolidadoDiario.Api.Autenticacao;
using ConsolidadoDiario.Aplicacao.Abstracoes;
using ConsolidadoDiario.Aplicacao.Services.ConsultarSaldoDiario;
using ConsolidadoDiario.Infraestrutura.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ConsolidadoDiario.Testes.Integracao.Infraestrutura;

public sealed class ConsolidadoDiarioApiFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private const string JwtChaveAssinatura = "chave-de-assinatura-dos-testes-com-32b";
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly DateTime _agoraUtc;
    private readonly int _atrasoMaximoToleradoEmMinutos;

    public ConsolidadoDiarioApiFactory(
        DateTime? agoraUtc = null,
        int atrasoMaximoToleradoEmMinutos = 5)
    {
        _agoraUtc = agoraUtc ?? new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc);
        _atrasoMaximoToleradoEmMinutos = atrasoMaximoToleradoEmMinutos;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ConsultarSaldoDiarioOpcoes.Secao}:{nameof(ConsultarSaldoDiarioOpcoes.AtrasoMaximoToleradoEmMinutos)}"] =
                    _atrasoMaximoToleradoEmMinutos.ToString(CultureInfo.InvariantCulture),
                [$"{AutenticacaoJwtOpcoes.Secao}:{nameof(AutenticacaoJwtOpcoes.ChaveAssinatura)}"] = JwtChaveAssinatura,
                [$"{AutenticacaoJwtOpcoes.Secao}:{nameof(AutenticacaoJwtOpcoes.ExpiracaoEmMinutos)}"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ConsolidadoDiarioDbContext>>();
            services.RemoveAll<ConsolidadoDiarioDbContext>();
            services.RemoveAll<IRelogioUtc>();

            services.AddDbContext<ConsolidadoDiarioDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddSingleton<IRelogioUtc>(new RelogioUtcFixo(_agoraUtc));
        });
    }

    public HttpClient CriarClientAutenticado(params string[] escopos)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GerarToken(escopos));
        return client;
    }

    public string GerarToken(params string[] escopos)
    {
        var emitido = DateTimeOffset.UtcNow.AddMinutes(-1);
        var expira = emitido.AddHours(1);

        return JwtTokenTesteHelper.GerarToken(
            JwtChaveAssinatura,
            escopos,
            emitido,
            expira);
    }

    public async Task InicializarBancoAsync()
    {
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidadoDiarioDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task ExecutarNoDbContextAsync(Func<ConsolidadoDiarioDbContext, Task> acao)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidadoDiarioDbContext>();

        await acao(dbContext);
    }

    public new async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
