using Lancamentos.Aplicacao.Abstracoes;
using Lancamentos.Aplicacao.Integracao;
using Lancamentos.Dominio.Entidades;
using Lancamentos.Dominio.ObjetosDeValor;
using System.Text.Json;

namespace Lancamentos.Aplicacao.Services.RegistrarLancamento;

public sealed class RegistrarLancamentoService
{
    private readonly IRegistroLancamentoRepositorio _repositorio;
    private readonly IRelogioUtc _relogioUtc;

    public RegistrarLancamentoService(
        IRegistroLancamentoRepositorio repositorio,
        IRelogioUtc relogioUtc)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        ArgumentNullException.ThrowIfNull(relogioUtc);

        _repositorio = repositorio;
        _relogioUtc = relogioUtc;
    }

    public async Task<LancamentoDto> ExecutarAsync(
        RegistrarLancamentoCommand comando,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var lancamento = Lancamento.Criar(
            TipoLancamento.Criar(comando.Tipo),
            ValorMonetario.Criar(comando.Valor),
            DataLancamento.Criar(comando.DataLancamento),
            _relogioUtc.UtcNow);

        var eventoId = Guid.NewGuid();
        var evento = LancamentoRegistradoV1.Criar(lancamento, eventoId, comando.CorrelacaoId);
        var outboxMessage = OutboxMessage.Criar(
            evento.EventoId,
            nameof(LancamentoRegistradoV1),
            JsonSerializer.Serialize(evento),
            comando.CorrelacaoId,
            evento.OcorridoEmUtc);

        await _repositorio.RegistrarAsync(lancamento, outboxMessage, cancellationToken);

        return LancamentoDto.FromEntity(lancamento);
    }
}
