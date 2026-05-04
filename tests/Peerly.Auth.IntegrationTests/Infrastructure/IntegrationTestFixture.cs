using System;
using System.Threading.Tasks;
using Npgsql;
using Peerly.Auth.IntegrationTests.Features.V1.Auth.ConfirmEmail.Infrastructure;
using Peerly.Auth.IntegrationTests.Features.V1.Auth.Login.Infrastructure;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private const string DatabaseName = "peerly-auth-tests";
    private const string DatabaseUsername = "peerly-auth-user";
    private const string DatabasePassword = "pwd";

    private Respawner _respawner = null!;
    private WebApplicationFactory? _applicationFactory;
    private NpgsqlDataSource? _dataSource;
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase(DatabaseName)
        .WithUsername(DatabaseUsername)
        .WithPassword(DatabasePassword)
        .Build();

    public WebApplicationFactory ApplicationFactory => _applicationFactory ?? throw new InvalidOperationException("Integration fixture is not initialized.");
    public NpgsqlDataSource DataSource => _dataSource ?? throw new InvalidOperationException("Integration fixture is not initialized.");
    public ConfirmEmailGrpcClient ConfirmEmailClient => ApplicationFactory.CreateConfirmEmailClient();
    public LoginGrpcClient LoginClient => ApplicationFactory.CreateLoginClient();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        _dataSource = NpgsqlDataSource.Create(_database.GetConnectionString());
        await PostgresMigrationRunner.ApplyAsync(_dataSource);

        await using var connection = await _dataSource.OpenConnectionAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });

        _applicationFactory = new WebApplicationFactory(
            _database.Hostname,
            _database.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort),
            DatabaseName,
            DatabaseUsername,
            DatabasePassword);
        await _applicationFactory.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_applicationFactory is not null)
        {
            await _applicationFactory.DisposeAsync();
        }

        if (_dataSource is not null)
        {
            await DataSource.DisposeAsync();
        }

        await _database.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = await DataSource.OpenConnectionAsync();
        await _respawner.ResetAsync(connection);
    }
}
