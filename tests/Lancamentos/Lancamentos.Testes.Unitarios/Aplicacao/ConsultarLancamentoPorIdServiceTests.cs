using Lancamentos.Aplicacao.Services.ConsultarLancamento;
using Lancamentos.Dominio.Entidades;
using Lancamentos.Dominio.ObjetosDeValor;
using Lancamentos.Testes.Unitarios.Doubles;

namespace Lancamentos.Testes.Unitarios.Aplicacao;

public sealed class ConsultarLancamentoPorIdServiceTests
{
    [Fact]
    public async Task DeveRetornarLancamentoQuandoEncontrado()
    {
        var repositorio = new LancamentosRepositorioEmMemoria();
        var lancamento = Lancamento.Criar(
            Guid.NewGuid(),
            TipoLancamento.Criar("Debito"),
            ValorMonetario.Criar(35.40m),
            DataLancamento.Criar(new DateOnly(2026, 3, 16)),
            new DateTime(2026, 3, 17, 10, 0, 0, DateTimeKind.Utc));

        await repositorio.AdicionarAsync(lancamento);

        var service = new ConsultarLancamentoPorIdService(repositorio);

        var resultado = await service.ExecutarAsync(lancamento.Id);

        Assert.NotNull(resultado);
        Assert.Equal(lancamento.Id, resultado.Id);
        Assert.Equal("Debito", resultado.Tipo);
    }

    [Fact]
    public async Task DeveRetornarNuloQuandoLancamentoNaoExistir()
    {
        var service = new ConsultarLancamentoPorIdService(new LancamentosRepositorioEmMemoria());

        var resultado = await service.ExecutarAsync(Guid.NewGuid());

        Assert.Null(resultado);
    }

    [Fact]
    public async Task DeveRejeitarIdentificadorVazio()
    {
        var service = new ConsultarLancamentoPorIdService(new LancamentosRepositorioEmMemoria());

        await Assert.ThrowsAsync<ArgumentException>(() => service.ExecutarAsync(Guid.Empty));
    }
}
