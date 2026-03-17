namespace FluxoDeCaixa.Testes.EndToEnd.Infraestrutura;

internal static class RepositorioRaizHelper
{
    public static string Obter()
    {
        var diretorioAtual = new DirectoryInfo(AppContext.BaseDirectory);

        while (diretorioAtual is not null)
        {
            if (File.Exists(Path.Combine(diretorioAtual.FullName, "FluxoDeCaixa.sln")))
            {
                return diretorioAtual.FullName;
            }

            diretorioAtual = diretorioAtual.Parent;
        }

        throw new InvalidOperationException("Nao foi possivel localizar a raiz do repositorio.");
    }
}
