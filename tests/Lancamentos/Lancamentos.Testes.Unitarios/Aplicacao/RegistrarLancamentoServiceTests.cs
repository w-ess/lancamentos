using Lancamentos.Aplicacao.Services.RegistrarLancamento;
using Lancamentos.Aplicacao.Integracao;
using Lancamentos.Dominio.Excecoes;
using Lancamentos.Testes.Unitarios.Doubles;
using System.Text.Json;

namespace Lancamentos.Testes.Unitarios.Aplicacao;

public sealed class RegistrarLancamentoServiceTests
{
    [Fact]
    public async Task DeveRegistrarLancamentoEMapearRetorno()
    {
        var repositorio = new LancamentosRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var service = new RegistrarLancamentoService(repositorio, relogio);
        var comando = new RegistrarLancamentoCommand(
            "Credito",
            99.90m,
            new DateOnly(2026, 3, 17),
            "correlacao-teste");

        var resultado = await service.ExecutarAsync(comando);
        var persistido = await repositorio.ObterPorIdAsync(resultado.Id);
        var outboxMessage = Assert.Single(repositorio.ListarMensagens());
        var evento = JsonSerializer.Deserialize<LancamentoRegistradoV1>(outboxMessage.Conteudo);

        Assert.NotNull(persistido);
        Assert.NotNull(evento);
        Assert.Equal("Credito", resultado.Tipo);
        Assert.Equal(99.90m, resultado.Valor);
        Assert.Equal(new DateOnly(2026, 3, 17), resultado.DataLancamento);
        Assert.Equal(relogio.UtcNow, resultado.RegistradoEmUtc);
        Assert.Equal(nameof(LancamentoRegistradoV1), outboxMessage.Tipo);
        Assert.Equal("correlacao-teste", outboxMessage.CorrelacaoId);
        Assert.Equal(resultado.Id, evento.LancamentoId);
        Assert.Equal("correlacao-teste", evento.CorrelacaoId);
    }

    [Fact]
    public async Task DevePropagarValidacaoDeDominioQuandoComandoForInvalido()
    {
        var repositorio = new LancamentosRepositorioEmMemoria();
        var relogio = new RelogioUtcFixo(new DateTime(2026, 3, 17, 15, 30, 0, DateTimeKind.Utc));
        var service = new RegistrarLancamentoService(repositorio, relogio);
        var comando = new RegistrarLancamentoCommand(
            "Transferencia",
            99.90m,
            new DateOnly(2026, 3, 17),
            "correlacao-teste");

        await Assert.ThrowsAsync<ExcecaoDominio>(() => service.ExecutarAsync(comando));
    }
}
