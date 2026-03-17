using Lancamentos.Infraestrutura.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lancamentos.Testes.Integracao.Infraestrutura;

public sealed class LancamentosApiFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LancamentosDbContext>>();
            services.RemoveAll<LancamentosDbContext>();

            services.AddDbContext<LancamentosDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    public async Task InicializarBancoAsync()
    {
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LancamentosDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
