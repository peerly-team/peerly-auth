using Peerly.Auth.IntegrationTests.Features.V1.Auth.Login.Infrastructure;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Login;

[Collection(IntegrationTestCollection.Name)]
public abstract class LoginIntegrationTestBase : AuthIntegrationTestBase
{
    protected LoginIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected LoginGrpcClient LoginClient => Fixture.LoginClient;
}
