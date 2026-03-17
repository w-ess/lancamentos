using ConsolidadoDiario.Aplicacao.Abstracoes;
using ConsolidadoDiario.Infraestrutura.Persistencia;
using ConsolidadoDiario.Infraestrutura.Repositorios;
using ConsolidadoDiario.Infraestrutura.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsolidadoDiario.Infraestrutura.Configuracao;

public static class ConfiguracaoInfraestrutura
{
    public static IServiceCollection AdicionarInfraestruturaConsolidadoDiario(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ConsolidadoDiarioDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string 'ConsolidadoDiarioDb' nao foi configurada.");
        }

        services.AddDbContext<ConsolidadoDiarioDbContext>(opcoes =>
            opcoes.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure()));

        services.AddScoped<IConsolidadoDiarioRepositorio, ConsolidadoDiarioRepositorio>();
        services.AddSingleton<IRelogioUtc, RelogioSistemaUtc>();

        return services;
    }
}
