using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lancamentos.Infraestrutura.Persistencia;

public static class InicializacaoBancoDadosLancamentosExtensions
{
    public static async Task MigrarBancoDadosLancamentosAsync(this IHost host, CancellationToken cancellationToken = default)
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
                    .CreateLogger("InicializacaoBancoDadosLancamentos");
                var dbContext = scope.ServiceProvider.GetRequiredService<LancamentosDbContext>();

                await dbContext.Database.MigrateAsync(cancellationToken);

                logger.LogInformation("Migracoes do banco de Lancamentos aplicadas com sucesso.");
                return;
            }
            catch (Exception ex) when (tentativa < quantidadeMaximaTentativas)
            {
                using var scope = host.Services.CreateScope();
                var logger = scope.ServiceProvider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("InicializacaoBancoDadosLancamentos");

                logger.LogWarning(
                    ex,
                    "Falha ao aplicar migracoes do banco de Lancamentos. Tentativa {Tentativa} de {Total}. Nova tentativa em {AtrasoEmSegundos}s.",
                    tentativa,
                    quantidadeMaximaTentativas,
                    atrasoEntreTentativas.TotalSeconds);

                await Task.Delay(atrasoEntreTentativas, cancellationToken);
            }
        }
    }
}
