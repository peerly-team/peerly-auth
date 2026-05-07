using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Peerly.Auth.V1;
using Xunit;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Register;

public sealed class RegisterIntegrationTests : RegisterIntegrationTestBase
{
    private const string StrongPassword = "CorrectPassword123!";
    private const string WeakPassword = "123";

    private readonly Fixture _fixture = new();

    public RegisterIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Theory]
    [InlineData(Role.Student)]
    [InlineData(Role.Teacher)]
    public async Task V1Register_ValidRequest_ShouldReturnTokenAndCreateUserRelatedData(Role role)
    {
        // Arrange
        var email = CreateEmailAddress();
        var request = _fixture.Build<V1RegisterRequest>()
            .With(request => request.Email, email)
            .With(request => request.Password, StrongPassword)
            .With(request => request.Role, role)
            .Create();

        // Act
        var response = await RegisterClient.V1RegisterAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1RegisterResponse.ResponseOneofCase.SuccessResponse);
        response.SuccessResponse.UserId.Should().BeGreaterThan(0);
        response.SuccessResponse.Token.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.SuccessResponse.Token.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var userId = response.SuccessResponse.UserId;

        var usersCount = await GetUsersCountByEmailAsync(email);
        usersCount.Should().Be(1);

        var sessionsCount = await GetSessionsCountAsync(userId);
        sessionsCount.Should().Be(1);

        var activeSessionsCount = await GetActiveSessionsCountAsync(userId);
        activeSessionsCount.Should().Be(1);

        var emailVerificationsCount = await GetEmailVerificationsCountAsync(userId);
        emailVerificationsCount.Should().Be(1);

        var outboxMessagesCount = await GetOutboxMessagesCountAsync(userId);
        outboxMessagesCount.Should().Be(1);
    }

    [Theory]
    [InlineData(Role.Student)]
    [InlineData(Role.Teacher)]
    public async Task V1Register_EmailAlreadyUsed_ShouldBeValidationError(Role role)
    {
        // Arrange
        var email = CreateEmailAddress();
        await AddUserInDbAsync(email, StrongPassword);
        var request = _fixture.Build<V1RegisterRequest>()
            .With(request => request.Email, email)
            .With(request => request.Password, StrongPassword)
            .With(request => request.Role, role)
            .Create();

        // Act
        var response = await RegisterClient.V1RegisterAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1RegisterResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainKey(string.Empty)
            .WhoseValue.ErrorMessage.Should().Contain("Пользователь с таким адресом электронной почты уже существует");

        var usersCount = await GetUsersCountByEmailAsync(email);
        usersCount.Should().Be(1);
    }

    [Theory]
    [InlineData(Role.Student)]
    [InlineData(Role.Teacher)]
    public async Task V1Register_PasswordTooSimple_ShouldBeValidationError(Role role)
    {
        // Arrange
        var email = CreateEmailAddress();
        var request = _fixture.Build<V1RegisterRequest>()
            .With(request => request.Email, email)
            .With(request => request.Password, WeakPassword)
            .With(request => request.Role, role)
            .Create();

        // Act
        var response = await RegisterClient.V1RegisterAsync(request);

        // Assert
        response.ResponseCase.Should().Be(V1RegisterResponse.ResponseOneofCase.ValidationError);
        response.ValidationError.Errors.Should().ContainKey(string.Empty)
            .WhoseValue.ErrorMessage.Should().Contain("Пароль слишком простой");

        var usersCount = await GetUsersCountByEmailAsync(email);
        usersCount.Should().Be(0);
    }

    [Fact]
    public async Task V1Register_InvalidEmail_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1RegisterRequest>()
            .With(request => request.Email, _fixture.Create<string>())
            .With(request => request.Password, StrongPassword)
            .With(request => request.Role, Role.Student)
            .Create();

        // Act
        var act = async () => await RegisterClient.V1RegisterAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Email));
    }

    [Fact]
    public async Task V1Register_EmptyPassword_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1RegisterRequest>()
            .With(request => request.Email, CreateEmailAddress)
            .With(request => request.Password, string.Empty)
            .With(request => request.Role, Role.Student)
            .Create();

        // Act
        var act = async () => await RegisterClient.V1RegisterAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Password));
    }

    [Fact]
    public async Task V1Register_EmptyName_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1RegisterRequest>()
            .With(request => request.Email, CreateEmailAddress)
            .With(request => request.Password, StrongPassword)
            .With(request => request.Name, string.Empty)
            .With(request => request.Role, Role.Student)
            .Create();

        // Act
        var act = async () => await RegisterClient.V1RegisterAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Name));
    }

    [Fact]
    public async Task V1Register_AdminRole_ShouldReturnInvalidArgument()
    {
        // Arrange
        var request = _fixture.Build<V1RegisterRequest>()
            .With(request => request.Email, CreateEmailAddress)
            .With(request => request.Password, StrongPassword)
            .With(request => request.Role, Role.Admin)
            .Create();

        // Act
        var act = async () => await RegisterClient.V1RegisterAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Which.Message.Should().Contain(nameof(request.Role));
    }

    private static string CreateEmailAddress() => $"user-{Guid.NewGuid():N}@peerly.test";
}
