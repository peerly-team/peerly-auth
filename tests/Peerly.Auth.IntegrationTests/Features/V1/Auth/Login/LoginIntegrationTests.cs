using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Peerly.Auth.V1;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Login;

public sealed class LoginIntegrationTests : LoginIntegrationTestBase
{
    private const string CorrectPassword = "CorrectPassword123!";

    private readonly Fixture _fixture = new();

    public LoginIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task V1Login_UserExistsAndPasswordCorrect_ShouldReturnTokenAndCreateSession()
    {
        // Arrange
        var email = CreateEmailAddress();
        var userId = await AddUserInDbAsync(email, CorrectPassword);
        var request = _fixture.Build<V1LoginRequest>()
            .With(request => request.Email, email)
            .With(request => request.Password, CorrectPassword)
            .Create();

        // Act
        var response = await LoginClient.V1LoginAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1LoginResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.UserId.Should().Be(userId);
        response.SuccessResponse.Token.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.SuccessResponse.Token.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var sessionsCount = await GetSessionsCountAsync(userId);
        sessionsCount.Should().Be(1);

        var activeSessionsCount = await GetActiveSessionsCountAsync(userId);
        activeSessionsCount.Should().Be(1);
    }

    [Fact]
    public async Task V1Login_UserHasActiveSession_ShouldCancelPreviousSessionAndCreateSession()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@peerly.test";
        var userId = await AddUserInDbAsync(email, CorrectPassword);
        await AddActiveSessionInDbAsync(userId);
        var request = _fixture.Build<V1LoginRequest>()
            .With(request => request.Email, email)
            .With(request => request.Password, CorrectPassword)
            .Create();

        // Act
        var response = await LoginClient.V1LoginAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1LoginResponse.ResponseOneofCase.SuccessResponse);

        var sessionsCount = await GetSessionsCountAsync(userId);
        sessionsCount.Should().Be(2);

        var activeSessionsCount = await GetActiveSessionsCountAsync(userId);
        activeSessionsCount.Should().Be(1);

        var cancelledSessionsCount = await GetCancelledSessionsCountAsync(userId);
        cancelledSessionsCount.Should().Be(1);
    }

    [Fact]
    public async Task V1Login_UserNotFound_ShouldBeValidationError()
    {
        // Arrange
        var request = _fixture.Build<V1LoginRequest>()
            .With(request => request.Email, CreateEmailAddress)
            .With(request => request.Password, CorrectPassword)
            .Create();

        // Act
        var response = await LoginClient.V1LoginAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1LoginResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainKey(string.Empty)
            .WhoseValue.ErrorMessage.Should().Contain("Неверный адрес электронной почты или пароль");
    }

    [Fact]
    public async Task V1Login_PasswordIncorrect_ShouldBeValidationError()
    {
        // Arrange
        var email = CreateEmailAddress();
        var userId = await AddUserInDbAsync(email, CorrectPassword);
        var request = _fixture.Build<V1LoginRequest>()
            .With(request => request.Email, email)
            .Create();

        // Act
        var response = await LoginClient.V1LoginAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1LoginResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainKey(string.Empty)
            .WhoseValue.ErrorMessage.Should().Contain("Неверный адрес электронной почты или пароль");

        var sessionsCount = await GetSessionsCountAsync(userId);
        sessionsCount.Should().Be(0);
    }

    [Fact]
    public async Task V1Login_InvalidEmail_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Create<V1LoginRequest>();

        // Act
        var act = async () => await LoginClient.V1LoginAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Email));
    }

    [Fact]
    public async Task V1Login_EmptyPassword_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1LoginRequest>()
            .With(request => request.Email, CreateEmailAddress)
            .With(request => request.Password, string.Empty)
            .Create();

        // Act
        var act = async () => await LoginClient.V1LoginAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Password));
    }

    private static string CreateEmailAddress() => $"user-{Guid.NewGuid():N}@peerly.test";
}
