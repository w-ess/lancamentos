using Lancamentos.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Lancamentos.Infraestrutura.Persistencia;

public sealed class LancamentosDbContext : DbContext
{
    public LancamentosDbContext(DbContextOptions<LancamentosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarcadorInfraestrutura).Assembly);
    }
}
