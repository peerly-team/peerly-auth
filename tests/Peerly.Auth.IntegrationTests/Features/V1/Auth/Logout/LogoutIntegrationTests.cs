using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Peerly.Auth.V1;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Logout;

public sealed class LogoutIntegrationTests : LogoutIntegrationTestBase
{
    private const string CorrectPassword = "CorrectPassword123!";

    private readonly Fixture _fixture = new();

    public LogoutIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1Logout_ActiveSessionExistsAndRefreshTokenCorrect_ShouldCancelSession()
    {
        // Arrange
        var email = CreateEmailAddress();
        var userId = await AddUserInDbAsync(email, CorrectPassword);
        var loginResponse = await LoginClient.V1LoginAsync(_fixture.Build<V1LoginRequest>()
            .With(result => result.Email, email)
            .With(result => result.Password, CorrectPassword)
            .Create());
        var request = _fixture.Build<V1LogoutRequest>()
            .With(result => result.UserId, userId)
            .With(result => result.RefreshToken, loginResponse.SuccessResponse.Token.RefreshToken)
            .Create();

        // Act
        var response = await LogoutClient.V1LogoutAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1LogoutResponse.ResponseOneofCase.SuccessResponse);

        var activeSessionsCount = await GetActiveSessionsCountAsync(userId);
        activeSessionsCount.Should().Be(0);

        var cancelledSessionsCount = await GetCancelledSessionsCountAsync(userId);
        cancelledSessionsCount.Should().Be(1);
    }

    [Fact]
    public async Task V1Logout_ActiveSessionNotFound_ShouldBeValidationError()
    {
        // Arrange
        var userId = await AddUserInDbAsync(CreateEmailAddress(), CorrectPassword);
        var request = _fixture.Build<V1LogoutRequest>()
            .With(request => request.UserId, userId)
            .With(request => request.RefreshToken, Guid.NewGuid().ToString("N"))
            .Create();

        // Act
        var response = await LogoutClient.V1LogoutAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1LogoutResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainKey(string.Empty)
            .WhoseValue.ErrorMessage.Should().ContainSingle(message => message.Contains("Нет активных сессий"));
    }

    [Fact]
    public async Task V1Logout_RefreshTokenIncorrect_ShouldBeValidationError()
    {
        // Arrange
        var email = CreateEmailAddress();
        var userId = await AddUserInDbAsync(email, CorrectPassword);
        _ = await LoginClient.V1LoginAsync(_fixture.Build<V1LoginRequest>()
            .With(result => result.Email, email)
            .With(result => result.Password, CorrectPassword)
            .Create());
        var request = _fixture.Build<V1LogoutRequest>()
            .With(request => request.UserId, userId)
            .With(request => request.RefreshToken, Guid.NewGuid().ToString("N"))
            .Create();

        // Act
        var response = await LogoutClient.V1LogoutAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1LogoutResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainKey(string.Empty)
            .WhoseValue.ErrorMessage.Should().ContainSingle(message => message.Contains("Токен обновления"));

        var activeSessionsCount = await GetActiveSessionsCountAsync(userId);
        activeSessionsCount.Should().Be(1);

        var cancelledSessionsCount = await GetCancelledSessionsCountAsync(userId);
        cancelledSessionsCount.Should().Be(0);
    }

    [Fact]
    public async Task V1Logout_InvalidUserId_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1LogoutRequest>()
            .With(request => request.UserId, 0)
            .With(request => request.RefreshToken, Guid.NewGuid().ToString("N"))
            .Create();

        // Act
        var act = async () => await LogoutClient.V1LogoutAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.UserId));
    }

    [Fact]
    public async Task V1Logout_EmptyRefreshToken_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1LogoutRequest>()
            .With(request => request.UserId, 1)
            .With(request => request.RefreshToken, string.Empty)
            .Create();

        // Act
        var act = async () => await LogoutClient.V1LogoutAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.RefreshToken));
    }

    private static string CreateEmailAddress() => $"user-{Guid.NewGuid():N}@peerly.test";
}
