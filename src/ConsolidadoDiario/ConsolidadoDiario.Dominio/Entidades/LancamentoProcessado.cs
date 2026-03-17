using ConsolidadoDiario.Dominio.Excecoes;
using ConsolidadoDiario.Dominio.ObjetosDeValor;

namespace ConsolidadoDiario.Dominio.Entidades;

public sealed class LancamentoProcessado
{
    private LancamentoProcessado(
        Guid lancamentoId,
        Guid eventoId,
        TipoLancamento tipo,
        ValorMonetario valor,
        DateOnly dataLancamento,
        string correlacaoId,
        DateTime ocorridoEmUtc,
        DateTime processadoEmUtc)
    {
        LancamentoId = lancamentoId;
        EventoId = eventoId;
        Tipo = tipo;
        Valor = valor;
        DataLancamento = dataLancamento;
        CorrelacaoId = correlacaoId;
        OcorridoEmUtc = ocorridoEmUtc;
        ProcessadoEmUtc = processadoEmUtc;
    }

    public Guid LancamentoId { get; }

    public Guid EventoId { get; }

    public TipoLancamento Tipo { get; }

    public ValorMonetario Valor { get; }

    public DateOnly DataLancamento { get; }

    public string CorrelacaoId { get; }

    public DateTime OcorridoEmUtc { get; }

    public DateTime ProcessadoEmUtc { get; }

    public static LancamentoProcessado Criar(
        Guid lancamentoId,
        Guid eventoId,
        TipoLancamento tipo,
        ValorMonetario valor,
        DateOnly dataLancamento,
        string correlacaoId,
        DateTime ocorridoEmUtc,
        DateTime processadoEmUtc)
    {
        if (lancamentoId == Guid.Empty)
        {
            throw new ExcecaoDominio("O identificador do lancamento processado e obrigatorio.");
        }

        if (eventoId == Guid.Empty)
        {
            throw new ExcecaoDominio("O identificador do evento processado e obrigatorio.");
        }

        ArgumentNullException.ThrowIfNull(tipo);
        ArgumentNullException.ThrowIfNull(valor);

        if (dataLancamento == default)
        {
            throw new ExcecaoDominio("A data do lancamento processado e obrigatoria.");
        }

        if (string.IsNullOrWhiteSpace(correlacaoId))
        {
            throw new ExcecaoDominio("O identificador de correlacao do lancamento processado e obrigatorio.");
        }

        ValidarUtc(ocorridoEmUtc, "A data de ocorrencia do evento processado deve estar em UTC.");
        ValidarUtc(processadoEmUtc, "A data de processamento do lancamento deve estar em UTC.");

        return new LancamentoProcessado(
            lancamentoId,
            eventoId,
            tipo,
            valor,
            dataLancamento,
            correlacaoId.Trim(),
            ocorridoEmUtc,
            processadoEmUtc);
    }

    private static void ValidarUtc(DateTime valor, string mensagemQuandoNaoUtc)
    {
        if (valor == default)
        {
            throw new ExcecaoDominio("As datas do lancamento processado sao obrigatorias.");
        }

        if (valor.Kind != DateTimeKind.Utc)
        {
            throw new ExcecaoDominio(mensagemQuandoNaoUtc);
        }
    }
}
