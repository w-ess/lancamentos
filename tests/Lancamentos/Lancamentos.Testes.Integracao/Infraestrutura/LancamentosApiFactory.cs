using System.Net.Http.Headers;
using Lancamentos.Api.Autenticacao;
using Lancamentos.Infraestrutura.Persistencia;
using Lancamentos.Infraestrutura.Mensageria;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lancamentos.Testes.Integracao.Infraestrutura;

public sealed class LancamentosApiFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private const string JwtChaveAssinatura = "chave-de-assinatura-dos-testes-com-32b";
    private readonly string _connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = $"file:lancamentos-tests-{Guid.NewGuid():N}",
        Mode = SqliteOpenMode.Memory,
        Cache = SqliteCacheMode.Shared
    }.ToString();
    private readonly SqliteConnection _keeperConnection;
    private readonly int _falhasRestantesPublicacao;

    public LancamentosApiFactory(int falhasRestantesPublicacao = 0)
    {
        _keeperConnection = new SqliteConnection(_connectionString);
        _falhasRestantesPublicacao = falhasRestantesPublicacao;
    }

    public PublicadorMensagensIntegracaoFalso PublicadorMensagens { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{OutboxMessagePublisherOptions.Secao}:{nameof(OutboxMessagePublisherOptions.AtrasoInicialEmMilissegundos)}"] = "500",
                [$"{OutboxMessagePublisherOptions.Secao}:{nameof(OutboxMessagePublisherOptions.IntervaloEmMilissegundos)}"] = "50",
                [$"{OutboxMessagePublisherOptions.Secao}:{nameof(OutboxMessagePublisherOptions.QuantidadePorLote)}"] = "20",
                [$"{AutenticacaoJwtOpcoes.Secao}:{nameof(AutenticacaoJwtOpcoes.ChaveAssinatura)}"] = JwtChaveAssinatura,
                [$"{AutenticacaoJwtOpcoes.Secao}:{nameof(AutenticacaoJwtOpcoes.ExpiracaoEmMinutos)}"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LancamentosDbContext>>();
            services.RemoveAll<LancamentosDbContext>();
            services.RemoveAll<IPublicadorMensagensIntegracao>();

            services.AddDbContext<LancamentosDbContext>(options =>
                options.UseSqlite(_connectionString));

            PublicadorMensagens.DefinirFalhasRestantes(_falhasRestantesPublicacao);
            services.AddSingleton(PublicadorMensagens);
            services.AddSingleton<IPublicadorMensagensIntegracao>(provider =>
                provider.GetRequiredService<PublicadorMensagensIntegracaoFalso>());
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
        var emitidoEmUtc = DateTimeOffset.UtcNow.AddMinutes(-1);
        var expiraEmUtc = emitidoEmUtc.AddHours(1);

        return JwtTokenTesteHelper.GerarToken(
            JwtChaveAssinatura,
            escopos,
            emitidoEmUtc,
            expiraEmUtc);
    }

    public async Task InicializarBancoAsync()
    {
        await _keeperConnection.OpenAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LancamentosDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task<T> ExecutarNoDbContextAsync<T>(Func<LancamentosDbContext, Task<T>> acao)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LancamentosDbContext>();

        return await acao(dbContext);
    }

    public new async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _keeperConnection.DisposeAsync();
    }
}
