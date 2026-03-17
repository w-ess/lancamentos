using Lancamentos.Dominio.Entidades;

namespace Lancamentos.Aplicacao.Abstracoes;

public interface IRegistroLancamentoRepositorio
{
    Task RegistrarAsync(
        Lancamento lancamento,
        MensagemSaida mensagemSaida,
        CancellationToken cancellationToken = default);
}
