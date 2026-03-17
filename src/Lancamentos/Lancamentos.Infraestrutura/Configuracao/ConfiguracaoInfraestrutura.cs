using Lancamentos.Aplicacao.Abstracoes;
using Lancamentos.Infraestrutura.Persistencia;
using Lancamentos.Infraestrutura.Repositorios;
using Lancamentos.Infraestrutura.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lancamentos.Infraestrutura.Configuracao;

public static class ConfiguracaoInfraestrutura
{
    public static IServiceCollection AdicionarInfraestruturaLancamentos(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LancamentosDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string 'LancamentosDb' nao foi configurada.");
        }

        services.AddDbContext<LancamentosDbContext>(opcoes =>
            opcoes.UseNpgsql(connectionString));

        services.AddScoped<ILancamentosRepositorio, LancamentosRepositorio>();
        services.AddSingleton<IRelogioUtc, RelogioSistemaUtc>();

        return services;
    }
}
