using Peerly.Auth.IntegrationTests.Features.V1.Auth.GetJwks.Infrastructure;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.GetJwks;

[Collection(IntegrationTestCollection.Name)]
public abstract class GetJwksIntegrationTestBase : AuthIntegrationTestBase
{
    protected GetJwksIntegrationTestBase(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    protected GetJwksGrpcClient GetJwksClient => Fixture.GetJwksClient;
}
