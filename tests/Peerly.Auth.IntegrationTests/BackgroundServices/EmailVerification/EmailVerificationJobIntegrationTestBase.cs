using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.IntegrationTests.BackgroundServices.EmailVerification.Infrastructure;
using Peerly.Auth.IntegrationTests.Features.V1.Auth;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Peerly.Auth.Models.EmailVerifications;
using Xunit;

namespace Peerly.Auth.IntegrationTests.BackgroundServices.EmailVerification;

[Collection(IntegrationTestCollection.Name)]
public abstract class EmailVerificationJobIntegrationTestBase : AuthIntegrationTestBase
{
    protected EmailVerificationJobIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected FakeEmailSender EmailSender => Fixture.EmailSender;

    public override async Task InitializeAsync()
    {
        EmailSender.Reset();
        await base.InitializeAsync();
    }

    protected async Task AddEmailVerificationInDbAsync(
        long userId,
        string token,
        string processStatus = "Created",
        int failCount = 0,
        DateTimeOffset? takenTime = null)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query =
            """
            insert into email_verifications (user_id, token, expiration_time, process_status, taken_time, fail_count, creation_time)
            values (@userId, @token, @expirationTime, @processStatus, @takenTime, @failCount, @creationTime);
            """;

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("token", token);
        command.Parameters.AddWithValue("expirationTime", DateTimeOffset.UtcNow.AddHours(1));
        command.Parameters.AddWithValue("processStatus", processStatus);
        command.Parameters.AddWithValue("takenTime", (object?)takenTime ?? DBNull.Value);
        command.Parameters.AddWithValue("failCount", failCount);
        command.Parameters.AddWithValue("creationTime", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    protected async Task<string> GetEmailVerificationStatusAsync(long userId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query = "select process_status from email_verifications where user_id = @userId;";

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("userId", userId);

        return (string)(await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException($"No email_verifications row found for userId={userId}."));
    }

    protected async Task<long> GetEmailVerificationFailCountAsync(long userId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query = "select fail_count from email_verifications where user_id = @userId;";

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("userId", userId);

        return (int)(await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException($"No email_verifications row found for userId={userId}."));
    }

    protected async Task<string?> GetEmailVerificationErrorAsync(long userId)
    {
        await using var connection = await Fixture.DataSource.OpenConnectionAsync();

        const string Query = "select error from email_verifications where user_id = @userId;";

        await using var command = new NpgsqlCommand(Query, connection);
        command.Parameters.AddWithValue("userId", userId);

        var result = await command.ExecuteScalarAsync();
        return result is DBNull or null ? null : (string)result;
    }

    protected async Task RunExecutorAsync(
        EmailVerificationJobItem jobItem,
        CancellationToken cancellationToken = default)
    {
        using var scope = Fixture.ApplicationFactory.Services.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<IExecutor<EmailVerificationJobItem>>();
        await executor.RunAsync(jobItem, cancellationToken);
    }
}
