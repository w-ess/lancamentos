using ConsolidadoDiario.Aplicacao.Abstracoes;
using ConsolidadoDiario.Aplicacao.Integracao;
using ConsolidadoDiario.Dominio.Entidades;
using ConsolidadoDiario.Dominio.ObjetosDeValor;

namespace ConsolidadoDiario.Aplicacao.CasosDeUso.ProcessarLancamentoRegistrado;

public sealed class ProcessarLancamentoRegistradoCasoDeUso
{
    private readonly IConsolidadoDiarioRepositorio _repositorio;
    private readonly IRelogioUtc _relogioUtc;

    public ProcessarLancamentoRegistradoCasoDeUso(
        IConsolidadoDiarioRepositorio repositorio,
        IRelogioUtc relogioUtc)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        ArgumentNullException.ThrowIfNull(relogioUtc);

        _repositorio = repositorio;
        _relogioUtc = relogioUtc;
    }

    public async Task<ResultadoProcessamentoLancamento> ExecutarAsync(
        LancamentoRegistradoV1 evento,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evento);

        if (await _repositorio.ExisteLancamentoProcessadoAsync(evento.LancamentoId, cancellationToken))
        {
            return new ResultadoProcessamentoLancamento(
                evento.LancamentoId,
                evento.DataLancamento,
                JaProcessado: true);
        }

        var tipo = TipoLancamento.Criar(evento.Tipo);
        var valor = ValorMonetario.Criar(evento.Valor);
        var processadoEmUtc = _relogioUtc.UtcNow;

        var saldoDiario = await _repositorio.ObterSaldoPorDataAsync(evento.DataLancamento, cancellationToken);
        if (saldoDiario is null)
        {
            saldoDiario = SaldoDiario.Criar(evento.DataLancamento, processadoEmUtc);
            await _repositorio.AdicionarSaldoAsync(saldoDiario, cancellationToken);
        }

        saldoDiario.AplicarLancamento(tipo, valor, processadoEmUtc);

        var lancamentoProcessado = LancamentoProcessado.Criar(
            evento.LancamentoId,
            evento.EventoId,
            tipo,
            valor,
            evento.DataLancamento,
            evento.CorrelacaoId,
            evento.OcorridoEmUtc,
            processadoEmUtc);

        await _repositorio.AdicionarLancamentoProcessadoAsync(lancamentoProcessado, cancellationToken);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return new ResultadoProcessamentoLancamento(
            evento.LancamentoId,
            evento.DataLancamento,
            JaProcessado: false);
    }
}
