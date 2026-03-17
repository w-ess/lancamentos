namespace FluxoDeCaixa.Testes.EndToEnd.Infraestrutura;

[CollectionDefinition(Nome, DisableParallelization = true)]
public sealed class InfraestruturaEndToEndCollection : ICollectionFixture<InfraestruturaEndToEndFixture>
{
    public const string Nome = "infraestrutura-end-to-end";
}
