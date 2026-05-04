using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Peerly.Auth.V1;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.ConfirmEmail;

public sealed class ConfirmEmailIntegrationTests : ConfirmEmailIntegrationTestBase
{
    private readonly Fixture _fixture = new();

    public ConfirmEmailIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1ConfirmEmail_TokenExistsAndNotExpired_ShouldConfirmEmail()
    {
        // Arrange
        var token = Guid.NewGuid().ToString("N");
        var request = _fixture.Build<V1ConfirmEmailRequest>()
            .With(result => result.Token, token)
            .Create();
        var userId = await AddUserWithEmailVerificationInDbAsync(token, DateTimeOffset.UtcNow.AddHours(1));

        // Act
        var response = await ConfirmEmailClient.V1ConfirmEmailAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1ConfirmEmailResponse.ResponseOneofCase.SuccessResponse);

        var isUserConfirmed = await IsUserConfirmedAsync(userId);
        isUserConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task V1ConfirmEmail_TokenNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var request = _fixture.Create<V1ConfirmEmailRequest>();

        // Act
        var response = await ConfirmEmailClient.V1ConfirmEmailAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1ConfirmEmailResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
    }

    [Fact]
    public async Task V1ConfirmEmail_TokenExpired_ShouldBeValidationError()
    {
        // Arrange
        var token = Guid.NewGuid().ToString("N");
        var request = _fixture.Build<V1ConfirmEmailRequest>()
            .With(result => result.Token, token)
            .Create();
        var userId = await AddUserWithEmailVerificationInDbAsync(token, DateTimeOffset.UtcNow.AddHours(-1));

        // Act
        var response = await ConfirmEmailClient.V1ConfirmEmailAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1ConfirmEmailResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainKey(string.Empty)
            .WhoseValue.ErrorMessage.Should().Contain("Срок действия ссылки для подтверждения истёк");

        var isUserConfirmed = await IsUserConfirmedAsync(userId);
        isUserConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task V1ConfirmEmail_EmptyToken_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1ConfirmEmailRequest>()
            .With(result => result.Token, string.Empty)
            .Create();

        // Act
        var act = async () => await ConfirmEmailClient.V1ConfirmEmailAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Token));
    }
}
