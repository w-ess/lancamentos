using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace FluxoDeCaixa.Testes.EndToEnd.Infraestrutura;

public sealed class InfraestruturaEndToEndFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("postgres")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public string JwtIssuer => "fluxodecaixa-e2e";
    public string JwtAudience => "fluxodecaixa-clientes-e2e";
    public string JwtChaveAssinatura => "chave-end-to-end-com-pelo-menos-32-bytes";
    public string ConnectionStringLancamentos =>
        $"{_postgres.GetConnectionString()};Database=lancamentos_db;Pooling=false";
    public string ConnectionStringConsolidado =>
        $"{_postgres.GetConnectionString()};Database=consolidado_db;Pooling=false";
    public string RabbitMqHost => _rabbitMq.Hostname;
    public int RabbitMqPort => _rabbitMq.GetMappedPublicPort(5672);

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbitMq.StartAsync();
        await GarantirBancoAsync("lancamentos_db");
        await GarantirBancoAsync("consolidado_db");
    }

    public async Task DisposeAsync()
    {
        await _rabbitMq.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public string GerarToken(params string[] escopos)
    {
        var emitidoEmUtc = new DateTimeOffset(new DateTime(2026, 3, 17, 16, 0, 0, DateTimeKind.Utc));

        return Autenticacao.JwtTokenTesteHelper.GerarToken(
            JwtIssuer,
            JwtAudience,
            JwtChaveAssinatura,
            escopos,
            emitidoEmUtc,
            emitidoEmUtc.AddHours(1));
    }

    private async Task GarantirBancoAsync(string nomeBanco)
    {
        await using var conexao = new NpgsqlConnection(_postgres.GetConnectionString());
        await conexao.OpenAsync();

        await using var comandoExiste = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @nome",
            conexao);
        comandoExiste.Parameters.AddWithValue("nome", nomeBanco);

        var existe = await comandoExiste.ExecuteScalarAsync() is not null;

        if (existe)
        {
            return;
        }

        await using var comandoCriacao = new NpgsqlCommand($"CREATE DATABASE \"{nomeBanco}\"", conexao);
        await comandoCriacao.ExecuteNonQueryAsync();
    }
}
