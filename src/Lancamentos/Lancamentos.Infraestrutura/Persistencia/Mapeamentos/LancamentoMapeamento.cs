using Lancamentos.Dominio.Entidades;
using Lancamentos.Dominio.ObjetosDeValor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lancamentos.Infraestrutura.Persistencia.Mapeamentos;

public sealed class LancamentoMapeamento : IEntityTypeConfiguration<Lancamento>
{
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.ToTable("lancamentos");

        builder.HasKey(lancamento => lancamento.Id);

        builder.Property(lancamento => lancamento.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(lancamento => lancamento.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(20)
            .HasConversion(
                tipo => tipo.Valor,
                valor => TipoLancamento.Criar(valor))
            .IsRequired();

        builder.Property(lancamento => lancamento.Valor)
            .HasColumnName("valor")
            .HasPrecision(18, 2)
            .HasConversion(
                valorMonetario => valorMonetario.Valor,
                valor => ValorMonetario.Criar(valor))
            .IsRequired();

        builder.Property(lancamento => lancamento.DataLancamento)
            .HasColumnName("data_lancamento")
            .HasConversion(
                dataLancamento => dataLancamento.Valor,
                valor => DataLancamento.Criar(valor))
            .IsRequired();

        builder.Property(lancamento => lancamento.RegistradoEmUtc)
            .HasColumnName("registrado_em_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
