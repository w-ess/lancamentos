using ConsolidadoDiario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace ConsolidadoDiario.Infraestrutura.Persistencia;

public sealed class ConsolidadoDiarioDbContext : DbContext
{
    public ConsolidadoDiarioDbContext(DbContextOptions<ConsolidadoDiarioDbContext> options)
        : base(options)
    {
    }

    public DbSet<SaldoDiario> SaldosDiarios => Set<SaldoDiario>();

    public DbSet<LancamentoProcessado> LancamentosProcessados => Set<LancamentoProcessado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarcadorInfraestrutura).Assembly);
    }
}
