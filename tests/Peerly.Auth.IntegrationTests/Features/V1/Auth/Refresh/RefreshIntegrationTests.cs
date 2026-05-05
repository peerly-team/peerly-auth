using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Peerly.Auth.V1;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Refresh;

public sealed class RefreshIntegrationTests : RefreshIntegrationTestBase
{
    private const string CorrectPassword = "CorrectPassword123!";

    private readonly Fixture _fixture = new();

    public RefreshIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1Refresh_ActiveSessionExistsAndRefreshTokenCorrect_ShouldReturnNewAccessTokenWithSameRefreshToken()
    {
        // Arrange
        var email = CreateEmailAddress();
        var userId = await AddUserInDbAsync(email, CorrectPassword);
        var refreshToken = Guid.NewGuid().ToString("N");
        await AddActiveSessionInDbAsync(userId, refreshToken);
        var request = _fixture.Build<V1RefreshRequest>()
            .With(request => request.UserId, userId)
            .With(request => request.RefreshToken, refreshToken)
            .Create();

        // Act
        var response = await RefreshClient.V1RefreshAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1RefreshResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.Token.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.SuccessResponse.Token.RefreshToken.Should().Be(refreshToken);

        var sessionsCount = await GetSessionsCountAsync(userId);
        sessionsCount.Should().Be(1);

        var activeSessionsCount = await GetActiveSessionsCountAsync(userId);
        activeSessionsCount.Should().Be(1);
    }

    [Fact]
    public async Task V1Refresh_ActiveSessionNotFound_ShouldBeValidationError()
    {
        // Arrange
        var userId = await AddUserInDbAsync(CreateEmailAddress(), CorrectPassword);
        var request = _fixture.Build<V1RefreshRequest>()
            .With(request => request.UserId, userId)
            .With(request => request.RefreshToken, Guid.NewGuid().ToString("N"))
            .Create();

        // Act
        var response = await RefreshClient.V1RefreshAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1RefreshResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainKey(string.Empty)
            .WhoseValue.ErrorMessage.Should().ContainSingle(message => message.Contains("Нет активных сессий"));
    }

    [Fact]
    public async Task V1Refresh_RefreshTokenIncorrect_ShouldBeValidationError()
    {
        // Arrange
        var email = CreateEmailAddress();
        var userId = await AddUserInDbAsync(email, CorrectPassword);
        await AddActiveSessionInDbAsync(userId, Guid.NewGuid().ToString("N"));
        var request = _fixture.Build<V1RefreshRequest>()
            .With(request => request.UserId, userId)
            .With(request => request.RefreshToken, Guid.NewGuid().ToString("N"))
            .Create();

        // Act
        var response = await RefreshClient.V1RefreshAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1RefreshResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainKey(string.Empty)
            .WhoseValue.ErrorMessage.Should().ContainSingle(message => message.Contains("Токен обновления"));

        var activeSessionsCount = await GetActiveSessionsCountAsync(userId);
        activeSessionsCount.Should().Be(1);
    }

    [Fact]
    public async Task V1Refresh_UserNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        const long UserId = 9_876_543_210;
        var refreshToken = Guid.NewGuid().ToString("N");
        await AddActiveSessionInDbAsync(UserId, refreshToken);
        var request = _fixture.Build<V1RefreshRequest>()
            .With(request => request.UserId, UserId)
            .With(request => request.RefreshToken, refreshToken)
            .Create();

        // Act
        var response = await RefreshClient.V1RefreshAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1RefreshResponse.ResponseOneofCase.OtherError);
        response.OtherError.Type.Should().Be(OtherError.Types.ErrorType.NotFound);
    }

    [Fact]
    public async Task V1Refresh_InvalidUserId_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1RefreshRequest>()
            .With(request => request.UserId, 0)
            .With(request => request.RefreshToken, Guid.NewGuid().ToString("N"))
            .Create();

        // Act
        var act = async () => await RefreshClient.V1RefreshAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.UserId));
    }

    [Fact]
    public async Task V1Refresh_EmptyRefreshToken_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1RefreshRequest>()
            .With(request => request.UserId, 1)
            .With(request => request.RefreshToken, string.Empty)
            .Create();

        // Act
        var act = async () => await RefreshClient.V1RefreshAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.RefreshToken));
    }

    private static string CreateEmailAddress() => $"user-{Guid.NewGuid():N}@peerly.test";
}
