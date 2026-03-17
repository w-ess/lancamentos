using Lancamentos.Dominio.Entidades;
using Lancamentos.Dominio.Excecoes;

namespace Lancamentos.Testes.Unitarios.Dominio;

public sealed class OutboxMessageTests
{
    [Fact]
    public void DeveCriarOutboxMessageValida()
    {
        var ocorridaEmUtc = new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc);

        var mensagem = OutboxMessage.Criar(
            Guid.NewGuid(),
            "LancamentoRegistradoV1",
            "{\"EventoId\":\"123\"}",
            "correlacao-123",
            ocorridaEmUtc);

        Assert.Equal("LancamentoRegistradoV1", mensagem.Tipo);
        Assert.Equal("correlacao-123", mensagem.CorrelacaoId);
        Assert.Equal(ocorridaEmUtc, mensagem.OcorridaEmUtc);
        Assert.False(mensagem.Publicada);
    }

    [Fact]
    public void DeveRegistrarFalhaDePublicacao()
    {
        var mensagem = OutboxMessage.Criar(
            Guid.NewGuid(),
            "LancamentoRegistradoV1",
            "{\"EventoId\":\"123\"}",
            "correlacao-123",
            new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc));

        mensagem.RegistrarFalhaPublicacao("falha temporaria");

        Assert.Equal(1, mensagem.TentativasPublicacao);
        Assert.Equal("falha temporaria", mensagem.UltimoErro);
        Assert.Null(mensagem.PublicadaEmUtc);
    }

    [Fact]
    public void DeveExigirOcorrenciaEmUtc()
    {
        var ocorridaEmLocal = new DateTime(2026, 3, 17, 13, 0, 0, DateTimeKind.Local);

        var excecao = Assert.Throws<ExcecaoDominio>(() => OutboxMessage.Criar(
            Guid.NewGuid(),
            "LancamentoRegistradoV1",
            "{\"EventoId\":\"123\"}",
            "correlacao-123",
            ocorridaEmLocal));

        Assert.Equal("A data de ocorrencia da mensagem da outbox deve estar em UTC.", excecao.Message);
    }
}
