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
    private const string JwtIssuer = "fluxodecaixa-testes";
    private const string JwtAudience = "fluxodecaixa-clientes-testes";
    private const string JwtChaveAssinatura = "chave-de-assinatura-dos-testes-com-32b";
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly int _falhasRestantesPublicacao;

    public LancamentosApiFactory(int falhasRestantesPublicacao = 0)
    {
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
                [$"{PublicadorMensagensSaidaOpcoes.Secao}:{nameof(PublicadorMensagensSaidaOpcoes.AtrasoInicialEmMilissegundos)}"] = "500",
                [$"{PublicadorMensagensSaidaOpcoes.Secao}:{nameof(PublicadorMensagensSaidaOpcoes.IntervaloEmMilissegundos)}"] = "50",
                [$"{PublicadorMensagensSaidaOpcoes.Secao}:{nameof(PublicadorMensagensSaidaOpcoes.QuantidadePorLote)}"] = "20",
                [$"{AutenticacaoJwtOpcoes.Secao}:{nameof(AutenticacaoJwtOpcoes.Issuer)}"] = JwtIssuer,
                [$"{AutenticacaoJwtOpcoes.Secao}:{nameof(AutenticacaoJwtOpcoes.Audience)}"] = JwtAudience,
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
                options.UseSqlite(_connection));

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
        var emitidoEmUtc = new DateTimeOffset(new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc));
        var expiraEmUtc = emitidoEmUtc.AddHours(1);

        return JwtTokenTesteHelper.GerarToken(
            JwtIssuer,
            JwtAudience,
            JwtChaveAssinatura,
            escopos,
            emitidoEmUtc,
            expiraEmUtc);
    }

    public async Task InicializarBancoAsync()
    {
        await _connection.OpenAsync();

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
        await _connection.DisposeAsync();
    }
}
