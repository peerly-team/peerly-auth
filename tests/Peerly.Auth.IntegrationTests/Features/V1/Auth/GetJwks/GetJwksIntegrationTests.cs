using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Peerly.Auth.V1;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.GetJwks;

public sealed class GetJwksIntegrationTests : GetJwksIntegrationTestBase
{
    public GetJwksIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1GetJwks_ShouldReturnOneJwkMatchingConfiguredKeys()
    {
        // Arrange
        var request = new V1GetJwksRequest();

        // Act
        var response = await GetJwksClient.V1GetJwksAsync(request);

        // Assert
        response.Jwks.Should().HaveCount(1);
        response.Jwks.Should().AllSatisfy(jwk => jwk.Should().NotBeNullOrWhiteSpace());
    }

    [Fact]
    public async Task V1GetJwks_ReturnedJwk_ShouldContainExpectedPublicKeyProperties()
    {
        // Arrange
        var request = new V1GetJwksRequest();

        // Act
        var response = await GetJwksClient.V1GetJwksAsync(request);

        // Assert
        using var document = JsonDocument.Parse(response.Jwks[0]);
        var root = document.RootElement;

        root.GetProperty("kty").GetString().Should().Be("RSA");
        root.GetProperty("use").GetString().Should().Be("sig");
        root.GetProperty("alg").GetString().Should().Be("RS256");
        root.GetProperty("kid").GetString().Should().Be("integration-test-key");
        root.GetProperty("n").GetString().Should().NotBeNullOrWhiteSpace();
        root.GetProperty("e").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task V1GetJwks_ReturnedJwk_ShouldNotContainPrivateKeyProperties()
    {
        // Arrange
        var request = new V1GetJwksRequest();

        // Act
        var response = await GetJwksClient.V1GetJwksAsync(request);

        // Assert
        using var document = JsonDocument.Parse(response.Jwks[0]);
        var root = document.RootElement;

        root.TryGetProperty("d", out _).Should().BeFalse();
        root.TryGetProperty("p", out _).Should().BeFalse();
        root.TryGetProperty("q", out _).Should().BeFalse();
        root.TryGetProperty("dp", out _).Should().BeFalse();
        root.TryGetProperty("dq", out _).Should().BeFalse();
        root.TryGetProperty("qi", out _).Should().BeFalse();
    }
}
