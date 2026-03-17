using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsolidadoDiario.Infraestrutura.Persistencia;

public static class InicializacaoBancoDadosConsolidadoDiarioExtensions
{
    public static async Task MigrarBancoDadosConsolidadoDiarioAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        const int quantidadeMaximaTentativas = 10;
        var atrasoEntreTentativas = TimeSpan.FromSeconds(3);

        for (var tentativa = 1; tentativa <= quantidadeMaximaTentativas; tentativa++)
        {
            try
            {
                using var scope = host.Services.CreateScope();
                var logger = scope.ServiceProvider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("InicializacaoBancoDadosConsolidadoDiario");
                var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidadoDiarioDbContext>();

                await dbContext.Database.MigrateAsync(cancellationToken);

                logger.LogInformation("Migracoes do banco de ConsolidadoDiario aplicadas com sucesso.");
                return;
            }
            catch (Exception ex) when (tentativa < quantidadeMaximaTentativas)
            {
                using var scope = host.Services.CreateScope();
                var logger = scope.ServiceProvider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("InicializacaoBancoDadosConsolidadoDiario");

                logger.LogWarning(
                    ex,
                    "Falha ao aplicar migracoes do banco de ConsolidadoDiario. Tentativa {Tentativa} de {Total}. Nova tentativa em {AtrasoEmSegundos}s.",
                    tentativa,
                    quantidadeMaximaTentativas,
                    atrasoEntreTentativas.TotalSeconds);

                await Task.Delay(atrasoEntreTentativas, cancellationToken);
            }
        }
    }
}
