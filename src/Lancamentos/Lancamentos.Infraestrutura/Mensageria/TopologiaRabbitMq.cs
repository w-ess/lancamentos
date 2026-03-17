using Lancamentos.Aplicacao.Integracao;

namespace Lancamentos.Infraestrutura.Mensageria;

internal static class TopologiaRabbitMq
{
    public const string ExchangeEventos = "lancamentos.eventos";
    public const string ExchangeMensagensMortas = "lancamentos.eventos.dlx";
    public const string RoutingKeyLancamentoRegistradoV1 = "lancamento.registrado.v1";
    public const string FilaLancamentoRegistradoV1 = "consolidado-diario.lancamento-registrado.v1";
    public const string FilaLancamentoRegistradoV1Dlq = "consolidado-diario.lancamento-registrado.v1.dlq";

    public static string ObterRoutingKey(string tipoMensagem)
    {
        return tipoMensagem switch
        {
            nameof(LancamentoRegistradoV1) => RoutingKeyLancamentoRegistradoV1,
            _ => throw new InvalidOperationException($"O tipo de mensagem '{tipoMensagem}' nao possui routing key configurada.")
        };
    }
}
