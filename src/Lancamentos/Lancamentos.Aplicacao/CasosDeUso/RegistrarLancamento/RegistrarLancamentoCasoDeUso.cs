using Lancamentos.Aplicacao.Abstracoes;
using Lancamentos.Dominio.Entidades;
using Lancamentos.Dominio.ObjetosDeValor;

namespace Lancamentos.Aplicacao.CasosDeUso.RegistrarLancamento;

public sealed class RegistrarLancamentoCasoDeUso
{
    private readonly ILancamentosRepositorio _repositorio;
    private readonly IRelogioUtc _relogioUtc;

    public RegistrarLancamentoCasoDeUso(
        ILancamentosRepositorio repositorio,
        IRelogioUtc relogioUtc)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        ArgumentNullException.ThrowIfNull(relogioUtc);

        _repositorio = repositorio;
        _relogioUtc = relogioUtc;
    }

    public async Task<LancamentoDto> ExecutarAsync(
        RegistrarLancamentoComando comando,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var lancamento = Lancamento.Criar(
            TipoLancamento.Criar(comando.Tipo),
            ValorMonetario.Criar(comando.Valor),
            DataLancamento.Criar(comando.DataLancamento),
            _relogioUtc.UtcNow);

        await _repositorio.AdicionarAsync(lancamento, cancellationToken);

        return LancamentoDto.DeEntidade(lancamento);
    }
}
