using Lancamentos.Aplicacao.CasosDeUso.RegistrarLancamento;
using Lancamentos.Aplicacao.Integracao;
using Lancamentos.Dominio.Excecoes;
using Lancamentos.Testes.Unitarios.Doubles;
using System.Text.Json;

namespace Lancamentos.Testes.Unitarios.Aplicacao;

public sealed class RegistrarLancamentoCasoDeUsoTests
{
    [Fact]
    public async Task DeveRegistrarLancamentoEMapearRetorno()
    {
        var repositorio = new LancamentosRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var casoDeUso = new RegistrarLancamentoCasoDeUso(repositorio, relogio);
        var comando = new RegistrarLancamentoComando(
            "Credito",
            99.90m,
            new DateOnly(2026, 3, 17),
            "correlacao-teste");

        var resultado = await casoDeUso.ExecutarAsync(comando);
        var persistido = await repositorio.ObterPorIdAsync(resultado.Id);
        var mensagemSaida = Assert.Single(repositorio.ListarMensagens());
        var evento = JsonSerializer.Deserialize<LancamentoRegistradoV1>(mensagemSaida.Conteudo);

        Assert.NotNull(persistido);
        Assert.NotNull(evento);
        Assert.Equal("Credito", resultado.Tipo);
        Assert.Equal(99.90m, resultado.Valor);
        Assert.Equal(new DateOnly(2026, 3, 17), resultado.DataLancamento);
        Assert.Equal(relogio.UtcNow, resultado.RegistradoEmUtc);
        Assert.Equal(nameof(LancamentoRegistradoV1), mensagemSaida.Tipo);
        Assert.Equal("correlacao-teste", mensagemSaida.CorrelacaoId);
        Assert.Equal(resultado.Id, evento.LancamentoId);
        Assert.Equal("correlacao-teste", evento.CorrelacaoId);
    }

    [Fact]
    public async Task DevePropagarValidacaoDeDominioQuandoComandoForInvalido()
    {
        var repositorio = new LancamentosRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var casoDeUso = new RegistrarLancamentoCasoDeUso(repositorio, relogio);
        var comando = new RegistrarLancamentoComando(
            "Transferencia",
            99.90m,
            new DateOnly(2026, 3, 17),
            "correlacao-teste");

        await Assert.ThrowsAsync<ExcecaoDominio>(() => casoDeUso.ExecutarAsync(comando));
    }
}
