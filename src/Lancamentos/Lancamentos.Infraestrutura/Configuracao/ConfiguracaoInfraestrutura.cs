using Lancamentos.Aplicacao.Abstracoes;
using Lancamentos.Infraestrutura.Mensageria;
using Lancamentos.Infraestrutura.Persistencia;
using Lancamentos.Infraestrutura.Repositorios;
using Lancamentos.Infraestrutura.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            opcoes.UseNpgsql(
                connectionString,
                npgsql => npgsql.EnableRetryOnFailure()));

        services.AddOptions<RabbitMqOpcoes>()
            .Bind(configuration.GetSection(RabbitMqOpcoes.Secao))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<PublicadorMensagensSaidaOpcoes>()
            .Bind(configuration.GetSection(PublicadorMensagensSaidaOpcoes.Secao))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<LancamentosRepositorio>();
        services.AddScoped<ILancamentosRepositorio>(provider => provider.GetRequiredService<LancamentosRepositorio>());
        services.AddScoped<IRegistroLancamentoRepositorio>(provider => provider.GetRequiredService<LancamentosRepositorio>());
        services.AddScoped<IMensagensSaidaRepositorio, MensagensSaidaRepositorio>();
        services.AddSingleton<IRelogioUtc, RelogioSistemaUtc>();
        services.AddSingleton<IPublicadorMensagensIntegracao, PublicadorRabbitMqMensagensIntegracao>();
        services.AddHostedService<PublicadorMensagensSaida>();

        return services;
    }
}
