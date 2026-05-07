using System.Threading.Tasks;
using Peerly.Auth.IntegrationTests.Features.V1.Auth.Register.Infrastructure;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Register;

[Collection(IntegrationTestCollection.Name)]
public abstract class RegisterIntegrationTestBase : AuthIntegrationTestBase
{
    protected RegisterIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected RegisterGrpcClient RegisterClient => Fixture.RegisterClient;

    protected Task<long> GetUsersCountByEmailAsync(string email)
    {
        return ExecuteScalarLongAsync(
            """
            select count(*)
              from users
             where email = @email;
            """,
            command => command.Parameters.AddWithValue("email", email));
    }

    protected Task<long> GetEmailVerificationsCountAsync(long userId)
    {
        return ExecuteScalarLongAsync(
            """
            select count(*)
              from email_verifications
             where user_id = @userId;
            """,
            command => command.Parameters.AddWithValue("userId", userId));
    }

    protected Task<long> GetOutboxMessagesCountAsync(long userId)
    {
        return ExecuteScalarLongAsync(
            """
            select count(*)
              from outbox_messages
             where key = @key;
            """,
            command => command.Parameters.AddWithValue("key", userId.ToString()));
    }
}
