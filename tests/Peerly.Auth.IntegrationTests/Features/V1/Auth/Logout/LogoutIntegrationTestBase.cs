using Peerly.Auth.IntegrationTests.Features.V1.Auth.Login;
using Peerly.Auth.IntegrationTests.Features.V1.Auth.Logout.Infrastructure;
using Peerly.Auth.IntegrationTests.Infrastructure;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Logout;

public abstract class LogoutIntegrationTestBase : LoginIntegrationTestBase
{
    protected LogoutIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected LogoutGrpcClient LogoutClient => Fixture.LogoutClient;
}
