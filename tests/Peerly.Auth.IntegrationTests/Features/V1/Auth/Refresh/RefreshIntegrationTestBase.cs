using Peerly.Auth.IntegrationTests.Features.V1.Auth.Refresh.Infrastructure;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Refresh;

[Collection(IntegrationTestCollection.Name)]
public abstract class RefreshIntegrationTestBase : AuthIntegrationTestBase
{
    protected RefreshIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected RefreshGrpcClient RefreshClient => Fixture.RefreshClient;
}
