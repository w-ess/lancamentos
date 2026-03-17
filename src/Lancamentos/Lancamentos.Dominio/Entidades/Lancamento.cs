using Lancamentos.Dominio.Excecoes;
using Lancamentos.Dominio.ObjetosDeValor;

namespace Lancamentos.Dominio.Entidades;

public sealed class Lancamento
{
    private Lancamento(
        Guid id,
        TipoLancamento tipo,
        ValorMonetario valor,
        DataLancamento dataLancamento,
        DateTime registradoEmUtc)
    {
        Id = id;
        Tipo = tipo;
        Valor = valor;
        DataLancamento = dataLancamento;
        RegistradoEmUtc = registradoEmUtc;
    }

    public Guid Id { get; }

    public TipoLancamento Tipo { get; }

    public ValorMonetario Valor { get; }

    public DataLancamento DataLancamento { get; }

    public DateTime RegistradoEmUtc { get; }

    public static Lancamento Criar(
        TipoLancamento tipo,
        ValorMonetario valor,
        DataLancamento dataLancamento,
        DateTime registradoEmUtc)
    {
        return Criar(Guid.NewGuid(), tipo, valor, dataLancamento, registradoEmUtc);
    }

    public static Lancamento Criar(
        Guid id,
        TipoLancamento tipo,
        ValorMonetario valor,
        DataLancamento dataLancamento,
        DateTime registradoEmUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ExcecaoDominio("O identificador do lancamento e obrigatorio.");
        }

        ArgumentNullException.ThrowIfNull(tipo);
        ArgumentNullException.ThrowIfNull(valor);
        ArgumentNullException.ThrowIfNull(dataLancamento);

        if (registradoEmUtc == default)
        {
            throw new ExcecaoDominio("A data de registro em UTC e obrigatoria.");
        }

        if (registradoEmUtc.Kind != DateTimeKind.Utc)
        {
            throw new ExcecaoDominio("A data de registro do lancamento deve estar em UTC.");
        }

        return new Lancamento(id, tipo, valor, dataLancamento, registradoEmUtc);
    }
}
