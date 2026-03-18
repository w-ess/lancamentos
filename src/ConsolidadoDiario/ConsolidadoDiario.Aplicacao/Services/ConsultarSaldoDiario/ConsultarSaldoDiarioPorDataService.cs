using ConsolidadoDiario.Aplicacao.Abstracoes;

namespace ConsolidadoDiario.Aplicacao.Services.ConsultarSaldoDiario;

public sealed class ConsultarSaldoDiarioPorDataService
{
    private readonly IConsolidadoDiarioRepositorio _repositorio;
    private readonly IRelogioUtc _relogioUtc;
    private readonly TimeSpan _toleranciaDefasagem;

    public ConsultarSaldoDiarioPorDataService(
        IConsolidadoDiarioRepositorio repositorio,
        IRelogioUtc relogioUtc,
        ConsultarSaldoDiarioOpcoes opcoes)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        ArgumentNullException.ThrowIfNull(relogioUtc);
        ArgumentNullException.ThrowIfNull(opcoes);

        _repositorio = repositorio;
        _relogioUtc = relogioUtc;
        _toleranciaDefasagem = opcoes.ObterToleranciaDefasagem();
    }

    public async Task<SaldoDiarioDto> ExecutarAsync(
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        if (data == default)
        {
            throw new ArgumentException("A data de consulta do saldo diario e obrigatoria.", nameof(data));
        }

        var agoraUtc = _relogioUtc.UtcNow;
        var saldoDiario = await _repositorio.ObterSaldoPorDataAsync(data, cancellationToken);
        var ultimaConfirmacaoUtc = await _repositorio.ObterUltimoProcessamentoUtcAsync(cancellationToken)
            ?? agoraUtc;

        return new SaldoDiarioDto(
            data,
            saldoDiario?.TotalCreditos ?? 0m,
            saldoDiario?.TotalDebitos ?? 0m,
            saldoDiario?.Saldo ?? 0m,
            saldoDiario?.Atualizado ?? ultimaConfirmacaoUtc,
            agoraUtc - ultimaConfirmacaoUtc > _toleranciaDefasagem);
    }
}
