using Processo.Lancamentos.Aplicacao.CasosDeUso.ConsultarLancamento;
using Processo.Lancamentos.Dominio.Entidades;
using Processo.Lancamentos.Dominio.ObjetosDeValor;
using Processo.Lancamentos.Testes.Unitarios.Doubles;

namespace Processo.Lancamentos.Testes.Unitarios.Aplicacao;

public sealed class ConsultarLancamentoPorIdCasoDeUsoTests
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

        var casoDeUso = new ConsultarLancamentoPorIdCasoDeUso(repositorio);

        var resultado = await casoDeUso.ExecutarAsync(lancamento.Id);

        Assert.NotNull(resultado);
        Assert.Equal(lancamento.Id, resultado.Id);
        Assert.Equal("Debito", resultado.Tipo);
    }

    [Fact]
    public async Task DeveRetornarNuloQuandoLancamentoNaoExistir()
    {
        var casoDeUso = new ConsultarLancamentoPorIdCasoDeUso(new LancamentosRepositorioEmMemoria());

        var resultado = await casoDeUso.ExecutarAsync(Guid.NewGuid());

        Assert.Null(resultado);
    }

    [Fact]
    public async Task DeveRejeitarIdentificadorVazio()
    {
        var casoDeUso = new ConsultarLancamentoPorIdCasoDeUso(new LancamentosRepositorioEmMemoria());

        await Assert.ThrowsAsync<ArgumentException>(() => casoDeUso.ExecutarAsync(Guid.Empty));
    }
}
