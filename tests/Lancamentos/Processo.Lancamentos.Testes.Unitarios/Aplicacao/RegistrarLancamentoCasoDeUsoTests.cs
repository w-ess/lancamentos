using Processo.Lancamentos.Aplicacao.CasosDeUso.RegistrarLancamento;
using Processo.Lancamentos.Dominio.Excecoes;
using Processo.Lancamentos.Testes.Unitarios.Doubles;

namespace Processo.Lancamentos.Testes.Unitarios.Aplicacao;

public sealed class RegistrarLancamentoCasoDeUsoTests
{
    [Fact]
    public async Task DeveRegistrarLancamentoEMapearRetorno()
    {
        var repositorio = new LancamentosRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var casoDeUso = new RegistrarLancamentoCasoDeUso(repositorio, relogio);
        var comando = new RegistrarLancamentoComando("Credito", 99.90m, new DateOnly(2026, 3, 17));

        var resultado = await casoDeUso.ExecutarAsync(comando);
        var persistido = await repositorio.ObterPorIdAsync(resultado.Id);

        Assert.NotNull(persistido);
        Assert.Equal("Credito", resultado.Tipo);
        Assert.Equal(99.90m, resultado.Valor);
        Assert.Equal(new DateOnly(2026, 3, 17), resultado.DataLancamento);
        Assert.Equal(relogio.UtcNow, resultado.RegistradoEmUtc);
    }

    [Fact]
    public async Task DevePropagarValidacaoDeDominioQuandoComandoForInvalido()
    {
        var repositorio = new LancamentosRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var casoDeUso = new RegistrarLancamentoCasoDeUso(repositorio, relogio);
        var comando = new RegistrarLancamentoComando("Transferencia", 99.90m, new DateOnly(2026, 3, 17));

        await Assert.ThrowsAsync<ExcecaoDominio>(() => casoDeUso.ExecutarAsync(comando));
    }
}
