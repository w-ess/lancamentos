using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lancamentos.Infraestrutura.Persistencia;

public sealed class FabricaLancamentosDbContextTempoDeDesign
    : IDesignTimeDbContextFactory<LancamentosDbContext>
{
    public LancamentosDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__LancamentosDb") ??
            "Host=localhost;Port=5432;Database=lancamentos_db;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<LancamentosDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LancamentosDbContext(optionsBuilder.Options);
    }
}
