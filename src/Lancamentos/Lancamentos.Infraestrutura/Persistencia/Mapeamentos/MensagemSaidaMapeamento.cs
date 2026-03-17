using Lancamentos.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lancamentos.Infraestrutura.Persistencia.Mapeamentos;

public sealed class MensagemSaidaMapeamento : IEntityTypeConfiguration<MensagemSaida>
{
    public void Configure(EntityTypeBuilder<MensagemSaida> builder)
    {
        builder.ToTable("mensagens_saida");

        builder.HasKey(mensagem => mensagem.Id);

        builder.Property(mensagem => mensagem.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(mensagem => mensagem.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(mensagem => mensagem.Conteudo)
            .HasColumnName("conteudo")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(mensagem => mensagem.CorrelacaoId)
            .HasColumnName("correlacao_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(mensagem => mensagem.OcorridaEmUtc)
            .HasColumnName("ocorrida_em_utc")
            .IsRequired();

        builder.Property(mensagem => mensagem.PublicadaEmUtc)
            .HasColumnName("publicada_em_utc");

        builder.Property(mensagem => mensagem.TentativasPublicacao)
            .HasColumnName("tentativas_publicacao")
            .IsRequired();

        builder.Property(mensagem => mensagem.UltimoErro)
            .HasColumnName("ultimo_erro")
            .HasMaxLength(2000);

        builder.Ignore(mensagem => mensagem.Publicada);

        builder.HasIndex(mensagem => new
            {
                mensagem.PublicadaEmUtc,
                mensagem.OcorridaEmUtc
            })
            .HasDatabaseName("ix_mensagens_saida_publicada_ocorrida");
    }
}
