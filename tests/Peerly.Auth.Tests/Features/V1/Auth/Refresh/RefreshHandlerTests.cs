using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Refresh;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;
using Peerly.Auth.Models.User;
using Xunit;

namespace Peerly.Auth.Tests.Features.V1.Auth.Refresh;

public sealed class RefreshHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyUserRepository> _userRepositoryMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<ICommandValidator<RefreshCommand, RefreshCommandResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly RefreshHandler _handler;

    public RefreshHandlerTests()
    {
        SetupUnitOfWorkFactory();

        _handler = new RefreshHandler(
            _unitOfWorkFactoryMock.Object,
            _tokenServiceMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldReturnNewAccessTokenWithSameRefreshToken()
    {
        // Arrange
        var command = _fixture.Create<RefreshCommand>();
        var userRole = _fixture.Create<UserRole>();
        var accessToken = _fixture.Create<string>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());

        _userRepositoryMock
            .Setup(repository => repository.GetUserRoleAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

        _tokenServiceMock
            .Setup(service => service.CreateAccessToken(command.UserId, userRole))
            .Returns(accessToken);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.AuthToken.AccessToken.Should().Be(accessToken);
        result.AsT0.AuthToken.RefreshToken.Should().Be(command.RefreshToken);

        _tokenServiceMock.Verify(
            service => service.CreateAccessToken(command.UserId, userRole),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationError_ShouldReturnValidationError()
    {
        // Arrange
        var command = _fixture.Create<RefreshCommand>();
        var validationError = ValidationError.From(_fixture.Create<string>());

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationError);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Should().Be(validationError);
        _unitOfWorkFactoryMock.Verify(
            factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _tokenServiceMock.Verify(
            service => service.CreateAccessToken(It.IsAny<Peerly.Auth.Identifiers.UserId>(), It.IsAny<UserRole>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_OtherError_ShouldReturnOtherError()
    {
        // Arrange
        var command = _fixture.Create<RefreshCommand>();
        var otherError = OtherError.NotFound();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherError);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Should().Be(otherError);
        _unitOfWorkFactoryMock.Verify(
            factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _tokenServiceMock.Verify(
            service => service.CreateAccessToken(It.IsAny<Peerly.Auth.Identifiers.UserId>(), It.IsAny<UserRole>()),
            Times.Never);
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyUserRepository)
            .Returns(_userRepositoryMock.Object);

        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }
}
