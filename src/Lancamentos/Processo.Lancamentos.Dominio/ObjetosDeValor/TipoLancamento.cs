using Processo.Lancamentos.Dominio.Excecoes;

namespace Processo.Lancamentos.Dominio.ObjetosDeValor;

public sealed record TipoLancamento
{
    public static readonly TipoLancamento Credito = new("Credito");
    public static readonly TipoLancamento Debito = new("Debito");

    public string Valor { get; }

    private TipoLancamento(string valor)
    {
        Valor = valor;
    }

    public bool EhCredito => Valor == Credito.Valor;

    public bool EhDebito => Valor == Debito.Valor;

    public static TipoLancamento Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ExcecaoDominio("O tipo de lancamento e obrigatorio.");
        }

        return valor.Trim().ToLowerInvariant() switch
        {
            "credito" => Credito,
            "debito" => Debito,
            _ => throw new ExcecaoDominio("O tipo de lancamento deve ser Credito ou Debito.")
        };
    }

    public override string ToString()
    {
        return Valor;
    }
}
