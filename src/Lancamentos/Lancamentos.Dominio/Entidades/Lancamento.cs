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
        DateTime registrado)
    {
        Id = id;
        Tipo = tipo;
        Valor = valor;
        DataLancamento = dataLancamento;
        Registrado = registrado;
    }

    public Guid Id { get; }

    public TipoLancamento Tipo { get; }

    public ValorMonetario Valor { get; }

    public DataLancamento DataLancamento { get; }

    public DateTime Registrado { get; }

    public static Lancamento Criar(
        TipoLancamento tipo,
        ValorMonetario valor,
        DataLancamento dataLancamento,
        DateTime registrado)
    {
        return Criar(Guid.NewGuid(), tipo, valor, dataLancamento, registrado);
    }

    public static Lancamento Criar(
        Guid id,
        TipoLancamento tipo,
        ValorMonetario valor,
        DataLancamento dataLancamento,
        DateTime registrado)
    {
        if (id == Guid.Empty)
        {
            throw new ExcecaoDominio("O identificador do lancamento e obrigatorio.");
        }

        ArgumentNullException.ThrowIfNull(tipo);
        ArgumentNullException.ThrowIfNull(valor);
        ArgumentNullException.ThrowIfNull(dataLancamento);

        if (registrado == default)
        {
            throw new ExcecaoDominio("A data de registro em UTC e obrigatoria.");
        }

        if (registrado.Kind != DateTimeKind.Utc)
        {
            throw new ExcecaoDominio("A data de registro do lancamento deve estar em UTC.");
        }

        return new Lancamento(id, tipo, valor, dataLancamento, registrado);
    }
}
