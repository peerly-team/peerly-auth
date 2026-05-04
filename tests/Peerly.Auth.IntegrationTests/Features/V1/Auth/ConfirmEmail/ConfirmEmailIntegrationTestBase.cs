using System;
using System.Threading.Tasks;
using Npgsql;
using Peerly.Auth.IntegrationTests.Features.V1.Auth.ConfirmEmail.Infrastructure;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.ConfirmEmail;

[Collection(IntegrationTestCollection.Name)]
public abstract class ConfirmEmailIntegrationTestBase : IAsyncLifetime
{
    protected ConfirmEmailIntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected ConfirmEmailGrpcClient ConfirmEmailClient => Fixture.ConfirmEmailClient;

    public virtual Task InitializeAsync()
    {
        return Fixture.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task<long> AddUserWithEmailVerificationInDbAsync(
        string token,
        DateTimeOffset expirationTime,
        bool isConfirmed = false)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string InsertUserQuery =
            """
            insert into users (email, password_hash, role, is_confirmed, creation_time)
            values (@email, @passwordHash, @role, @isConfirmed, @creationTime)
            returning id;
            """;

        await using var insertUserCommand = new NpgsqlCommand(InsertUserQuery, connection);
        insertUserCommand.Parameters.AddWithValue("email", $"user-{Guid.NewGuid():N}@peerly.test");
        insertUserCommand.Parameters.AddWithValue("passwordHash", Guid.NewGuid().ToString("N"));
        insertUserCommand.Parameters.AddWithValue("role", "Student");
        insertUserCommand.Parameters.AddWithValue("isConfirmed", isConfirmed);
        insertUserCommand.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        var userId = (long)(await insertUserCommand.ExecuteScalarAsync() ?? throw new InvalidOperationException("User id was not returned."));

        const string InsertEmailVerificationQuery =
            """
            insert into email_verifications (
                        user_id,
                        token,
                        expiration_time,
                        process_status,
                        fail_count,
                        creation_time)
                 values (
                        @userId,
                        @token,
                        @expirationTime,
                        @processStatus,
                        @failCount,
                        @creationTime);
            """;

        await using var insertEmailVerificationCommand = new NpgsqlCommand(InsertEmailVerificationQuery, connection);
        insertEmailVerificationCommand.Parameters.AddWithValue("userId", userId);
        insertEmailVerificationCommand.Parameters.AddWithValue("token", token);
        insertEmailVerificationCommand.Parameters.AddWithValue("expirationTime", expirationTime);
        insertEmailVerificationCommand.Parameters.AddWithValue("processStatus", "Created");
        insertEmailVerificationCommand.Parameters.AddWithValue("failCount", 0);
        insertEmailVerificationCommand.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        await insertEmailVerificationCommand.ExecuteNonQueryAsync();

        return userId;
    }

    protected async Task<bool> IsUserConfirmedAsync(long userId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            select is_confirmed
              from users
             where id = @userId;
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("userId", userId);

        return (bool)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException("User was not found."));
    }
}
