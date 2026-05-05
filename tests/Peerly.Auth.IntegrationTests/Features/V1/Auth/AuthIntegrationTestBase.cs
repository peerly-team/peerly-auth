using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth;

public abstract class AuthIntegrationTestBase : IAsyncLifetime
{
    protected AuthIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    public virtual Task InitializeAsync()
    {
        return Fixture.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task<long> AddUserInDbAsync(
        string email,
        string password,
        string role = "Student",
        bool isConfirmed = true)
    {
        var passwordHash = await HashAsync(password);

        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into users (email, password_hash, role, is_confirmed, creation_time)
            values (@email, @passwordHash, @role, @isConfirmed, @creationTime)
            returning id;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("email", email);
        command.Parameters.AddWithValue("passwordHash", passwordHash);
        command.Parameters.AddWithValue("role", role);
        command.Parameters.AddWithValue("isConfirmed", isConfirmed);
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        return (long)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException("User id was not returned."));
    }

    protected Task<long> AddActiveSessionInDbAsync(long userId)
    {
        return AddActiveSessionWithHashInDbAsync(userId, Guid.NewGuid().ToString("N"));
    }

    protected async Task<long> AddActiveSessionInDbAsync(long userId, string refreshToken)
    {
        var refreshTokenHash = await HashAsync(refreshToken);
        return await AddActiveSessionWithHashInDbAsync(userId, refreshTokenHash);
    }

    protected Task<long> GetSessionsCountAsync(long userId)
    {
        return ExecuteScalarLongAsync(
            """
            select count(*)
              from sessions
             where user_id = @userId;
            """,
            userId);
    }

    protected Task<long> GetActiveSessionsCountAsync(long userId)
    {
        return ExecuteScalarLongAsync(
            """
            select count(*)
              from sessions
             where user_id = @userId
               and cancellation_time is null;
            """,
            userId);
    }

    protected Task<long> GetCancelledSessionsCountAsync(long userId)
    {
        return ExecuteScalarLongAsync(
            """
            select count(*)
              from sessions
             where user_id = @userId
               and cancellation_time is not null;
            """,
            userId);
    }

    protected async Task<string> HashAsync(string value)
    {
        using var scope = Fixture.ApplicationFactory.Services.CreateScope();
        var hashServiceType = typeof(Peerly.Auth.ApplicationServices.Extensions.ServiceCollectionExtensions).Assembly.GetType(
            "Peerly.Auth.ApplicationServices.Services.Abstractions.IHashService",
            throwOnError: true)
            ?? throw new InvalidOperationException("Hash service type was not found.");
        var hashService = scope.ServiceProvider.GetRequiredService(hashServiceType);
        var hashAsyncMethod = hashServiceType.GetMethod("HashAsync", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("HashAsync method was not found.");

        var hashTask = (Task<string>?)hashAsyncMethod.Invoke(hashService, [value, CancellationToken.None])
            ?? throw new InvalidOperationException("HashAsync method did not return a task.");

        return await hashTask;
    }

    private async Task<long> AddActiveSessionWithHashInDbAsync(long userId, string refreshTokenHash)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into sessions (user_id, refresh_token_hash, expiration_time, creation_time)
            values (@userId, @refreshTokenHash, @expirationTime, @creationTime)
            returning id;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("refreshTokenHash", refreshTokenHash);
        command.Parameters.AddWithValue("expirationTime", DateTimeOffset.UtcNow.AddDays(1));
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        return (long)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException("Session id was not returned."));
    }

    private async Task<long> ExecuteScalarLongAsync(string query, long userId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("userId", userId);

        return (long)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException("Scalar query did not return a value."));
    }
}
