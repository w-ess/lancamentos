using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConsolidadoDiario.Infraestrutura.Persistencia;

public sealed class FabricaConsolidadoDiarioDbContextTempoDeDesign
    : IDesignTimeDbContextFactory<ConsolidadoDiarioDbContext>
{
    public ConsolidadoDiarioDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__ConsolidadoDiarioDb") ??
            "Host=localhost;Port=5432;Database=consolidado_db;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ConsolidadoDiarioDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ConsolidadoDiarioDbContext(optionsBuilder.Options);
    }
}
