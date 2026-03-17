namespace ConsolidadoDiario.Aplicacao.CasosDeUso;

public sealed record SaldoDiarioDto(
    DateOnly Data,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal Saldo,
    DateTime AtualizadoEmUtc,
    bool Defasado);
