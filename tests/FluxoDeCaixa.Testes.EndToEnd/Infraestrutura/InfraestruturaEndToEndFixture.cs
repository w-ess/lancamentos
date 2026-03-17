using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
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
        var emitidoEmUtc = DateTimeOffset.UtcNow.AddMinutes(-1);

        return Autenticacao.JwtTokenTesteHelper.GerarToken(
            JwtChaveAssinatura,
            escopos,
            emitidoEmUtc,
            emitidoEmUtc.AddHours(1));
    }

    public async Task ResetarEstadoAsync()
    {
        await LimparBancoLancamentosAsync();
        await LimparBancoConsolidadoAsync();
        await LimparFilasRabbitMqAsync();
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

    private async Task LimparBancoLancamentosAsync()
    {
        await using var conexao = new NpgsqlConnection(ConnectionStringLancamentos);
        await conexao.OpenAsync();

        if (!await TabelaExisteAsync(conexao, "lancamentos"))
        {
            return;
        }

        const string sql = """
                           TRUNCATE TABLE outbox_messages, lancamentos RESTART IDENTITY;
                           """;

        await using var comando = new NpgsqlCommand(sql, conexao);
        await comando.ExecuteNonQueryAsync();
    }

    private async Task LimparBancoConsolidadoAsync()
    {
        await using var conexao = new NpgsqlConnection(ConnectionStringConsolidado);
        await conexao.OpenAsync();

        if (!await TabelaExisteAsync(conexao, "saldos_diarios"))
        {
            return;
        }

        const string sql = """
                           TRUNCATE TABLE lancamentos_processados, saldos_diarios RESTART IDENTITY;
                           """;

        await using var comando = new NpgsqlCommand(sql, conexao);
        await comando.ExecuteNonQueryAsync();
    }

    private async Task LimparFilasRabbitMqAsync()
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = RabbitMqHost,
            Port = RabbitMqPort,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        };

        await using var connection = await connectionFactory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await TentarLimparFilaAsync(channel, "consolidado-diario.lancamento-registrado.v1");
        await TentarLimparFilaAsync(channel, "consolidado-diario.lancamento-registrado.v1.dlq");
    }

    private static async Task<bool> TabelaExisteAsync(NpgsqlConnection conexao, string nomeTabela)
    {
        await using var comando = new NpgsqlCommand(
            """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = @nomeTabela
            )
            """,
            conexao);
        comando.Parameters.AddWithValue("nomeTabela", nomeTabela);

        return await comando.ExecuteScalarAsync() is true;
    }

    private static async Task TentarLimparFilaAsync(IChannel channel, string nomeFila)
    {
        try
        {
            await channel.QueuePurgeAsync(nomeFila);
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
        {
        }
    }
}
