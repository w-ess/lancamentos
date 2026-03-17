using ConsolidadoDiario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsolidadoDiario.Infraestrutura.Persistencia.Mapeamentos;

public sealed class SaldoDiarioMapeamento : IEntityTypeConfiguration<SaldoDiario>
{
    public void Configure(EntityTypeBuilder<SaldoDiario> builder)
    {
        builder.ToTable("saldos_diarios");

        builder.HasKey(saldo => saldo.Data);

        builder.Property(saldo => saldo.Data)
            .HasColumnName("data")
            .ValueGeneratedNever();

        builder.Property(saldo => saldo.TotalCreditos)
            .HasColumnName("total_creditos")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(saldo => saldo.TotalDebitos)
            .HasColumnName("total_debitos")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(saldo => saldo.Saldo)
            .HasColumnName("saldo")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(saldo => saldo.AtualizadoEmUtc)
            .HasColumnName("atualizado_em_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
