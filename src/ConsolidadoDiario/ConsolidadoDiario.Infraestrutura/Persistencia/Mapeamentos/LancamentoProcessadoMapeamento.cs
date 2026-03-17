using ConsolidadoDiario.Dominio.Entidades;
using ConsolidadoDiario.Dominio.ObjetosDeValor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsolidadoDiario.Infraestrutura.Persistencia.Mapeamentos;

public sealed class LancamentoProcessadoMapeamento : IEntityTypeConfiguration<LancamentoProcessado>
{
    public void Configure(EntityTypeBuilder<LancamentoProcessado> builder)
    {
        builder.ToTable("lancamentos_processados");

        builder.HasKey(lancamento => lancamento.LancamentoId);

        builder.Property(lancamento => lancamento.LancamentoId)
            .HasColumnName("lancamento_id")
            .ValueGeneratedNever();

        builder.Property(lancamento => lancamento.EventoId)
            .HasColumnName("evento_id")
            .IsRequired();

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
            .IsRequired();

        builder.Property(lancamento => lancamento.CorrelacaoId)
            .HasColumnName("correlacao_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(lancamento => lancamento.OcorridoEmUtc)
            .HasColumnName("ocorrido_em_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(lancamento => lancamento.ProcessadoEmUtc)
            .HasColumnName("processado_em_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(lancamento => lancamento.EventoId)
            .HasDatabaseName("ix_lancamentos_processados_evento_id");

        builder.HasIndex(lancamento => lancamento.ProcessadoEmUtc)
            .HasDatabaseName("ix_lancamentos_processados_processado_em_utc");
    }
}
