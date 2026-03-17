using ConsolidadoDiario.Infraestrutura.Persistencia;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConsolidadoDiario.Testes.Integracao.Infraestrutura;

public sealed class ConsolidadoDiarioDbContextFactory : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private DbContextOptions<ConsolidadoDiarioDbContext>? _options;

    public async Task InicializarAsync()
    {
        await _connection.OpenAsync();

        _options = new DbContextOptionsBuilder<ConsolidadoDiarioDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using var dbContext = CriarDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public ConsolidadoDiarioDbContext CriarDbContext()
    {
        if (_options is null)
        {
            throw new InvalidOperationException("A factory do DbContext precisa ser inicializada antes do uso.");
        }

        return new ConsolidadoDiarioDbContext(_options);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
