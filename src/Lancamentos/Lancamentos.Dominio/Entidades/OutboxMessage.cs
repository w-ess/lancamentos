using Lancamentos.Dominio.Excecoes;

namespace Lancamentos.Dominio.Entidades;

public sealed class OutboxMessage
{
    private const int TamanhoMaximoUltimoErro = 2000;

    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        string tipo,
        string conteudo,
        string correlacaoId,
        DateTime ocorridaEmUtc)
    {
        Id = id;
        Tipo = tipo;
        Conteudo = conteudo;
        CorrelacaoId = correlacaoId;
        OcorridaEmUtc = ocorridaEmUtc;
    }

    public Guid Id { get; private set; }

    public string Tipo { get; private set; } = string.Empty;

    public string Conteudo { get; private set; } = string.Empty;

    public string CorrelacaoId { get; private set; } = string.Empty;

    public DateTime OcorridaEmUtc { get; private set; }

    public DateTime? PublicadaEmUtc { get; private set; }

    public int TentativasPublicacao { get; private set; }

    public string? UltimoErro { get; private set; }

    public bool Publicada => PublicadaEmUtc.HasValue;

    public static OutboxMessage Criar(
        Guid id,
        string tipo,
        string conteudo,
        string correlacaoId,
        DateTime ocorridaEmUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ExcecaoDominio("O identificador da mensagem da outbox e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(tipo))
        {
            throw new ExcecaoDominio("O tipo da mensagem da outbox e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(conteudo))
        {
            throw new ExcecaoDominio("O conteudo da mensagem da outbox e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(correlacaoId))
        {
            throw new ExcecaoDominio("O identificador de correlacao da mensagem da outbox e obrigatorio.");
        }

        if (ocorridaEmUtc == default)
        {
            throw new ExcecaoDominio("A data de ocorrencia da mensagem da outbox e obrigatoria.");
        }

        if (ocorridaEmUtc.Kind != DateTimeKind.Utc)
        {
            throw new ExcecaoDominio("A data de ocorrencia da mensagem da outbox deve estar em UTC.");
        }

        return new OutboxMessage(
            id,
            tipo.Trim(),
            conteudo,
            correlacaoId.Trim(),
            ocorridaEmUtc);
    }

    public void MarcarComoPublicada(DateTime publicadaEmUtc)
    {
        if (publicadaEmUtc == default)
        {
            throw new ExcecaoDominio("A data de publicacao da mensagem da outbox e obrigatoria.");
        }

        if (publicadaEmUtc.Kind != DateTimeKind.Utc)
        {
            throw new ExcecaoDominio("A data de publicacao da mensagem da outbox deve estar em UTC.");
        }

        PublicadaEmUtc = publicadaEmUtc;
        UltimoErro = null;
    }

    public void RegistrarFalhaPublicacao(string erro)
    {
        if (string.IsNullOrWhiteSpace(erro))
        {
            throw new ExcecaoDominio("A descricao do erro de publicacao e obrigatoria.");
        }

        TentativasPublicacao++;
        UltimoErro = erro.Length <= TamanhoMaximoUltimoErro
            ? erro
            : erro[..TamanhoMaximoUltimoErro];
    }
}
