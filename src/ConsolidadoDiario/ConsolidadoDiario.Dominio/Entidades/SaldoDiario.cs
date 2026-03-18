using ConsolidadoDiario.Dominio.Excecoes;
using ConsolidadoDiario.Dominio.ObjetosDeValor;

namespace ConsolidadoDiario.Dominio.Entidades;

public sealed class SaldoDiario
{
    private SaldoDiario(
        DateOnly data,
        decimal totalCreditos,
        decimal totalDebitos,
        decimal saldo,
        DateTime atualizado)
    {
        Data = data;
        TotalCreditos = totalCreditos;
        TotalDebitos = totalDebitos;
        Saldo = saldo;
        Atualizado = atualizado;
    }

    public DateOnly Data { get; }

    public decimal TotalCreditos { get; private set; }

    public decimal TotalDebitos { get; private set; }

    public decimal Saldo { get; private set; }

    public DateTime Atualizado { get; private set; }

    public static SaldoDiario Criar(DateOnly data, DateTime atualizado)
    {
        return Criar(data, 0m, 0m, atualizado);
    }

    public static SaldoDiario Criar(
        DateOnly data,
        decimal totalCreditos,
        decimal totalDebitos,
        DateTime atualizado)
    {
        if (data == default)
        {
            throw new ExcecaoDominio("A data do saldo diario e obrigatoria.");
        }

        ValidarValorAgregado(totalCreditos, "O total de creditos do saldo diario nao pode ser negativo.");
        ValidarValorAgregado(totalDebitos, "O total de debitos do saldo diario nao pode ser negativo.");
        ValidarUtc(atualizado, "A data de atualizacao do saldo diario deve estar em UTC.");

        return new SaldoDiario(
            data,
            totalCreditos,
            totalDebitos,
            totalCreditos - totalDebitos,
            atualizado);
    }

    public void AplicarLancamento(
        TipoLancamento tipo,
        ValorMonetario valor,
        DateTime atualizado)
    {
        ArgumentNullException.ThrowIfNull(tipo);
        ArgumentNullException.ThrowIfNull(valor);

        ValidarUtc(atualizado, "A data de atualizacao do saldo diario deve estar em UTC.");

        if (tipo.EhCredito)
        {
            TotalCreditos += valor.Valor;
        }
        else
        {
            TotalDebitos += valor.Valor;
        }

        Saldo = TotalCreditos - TotalDebitos;
        Atualizado = atualizado;
    }

    private static void ValidarValorAgregado(decimal valor, string mensagemQuandoNegativo)
    {
        if (valor < 0)
        {
            throw new ExcecaoDominio(mensagemQuandoNegativo);
        }

        if (ObterCasasDecimais(valor) > 2)
        {
            throw new ExcecaoDominio("Os valores agregados do saldo diario devem ter no maximo duas casas decimais.");
        }
    }

    private static void ValidarUtc(DateTime valor, string mensagemQuandoNaoUtc)
    {
        if (valor == default)
        {
            throw new ExcecaoDominio("A data de atualizacao do saldo diario e obrigatoria.");
        }

        if (valor.Kind != DateTimeKind.Utc)
        {
            throw new ExcecaoDominio(mensagemQuandoNaoUtc);
        }
    }

    private static int ObterCasasDecimais(decimal valor)
    {
        var bits = decimal.GetBits(valor);
        return (bits[3] >> 16) & 0x7F;
    }
}
