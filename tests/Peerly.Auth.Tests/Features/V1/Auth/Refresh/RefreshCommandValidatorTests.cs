using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Refresh;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.Models.Sessions;
using Peerly.Auth.Models.User;
using Xunit;

namespace Peerly.Auth.Tests.Features.V1.Auth.Refresh;

public sealed class RefreshCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlySessionRepository> _sessionRepositoryMock = new();
    private readonly Mock<IReadOnlyUserRepository> _userRepositoryMock = new();
    private readonly Mock<IHashService> _hashServiceMock = new();

    private readonly Fixture _fixture = new();
    private readonly RefreshCommandValidator _validator;

    public RefreshCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new RefreshCommandValidator(unitOfWorkFactory, _hashServiceMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ActiveSessionExistsAndRefreshTokenCorrectAndUserExists_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<RefreshCommand>();
        var session = _fixture.Create<Session>();
        var userRole = _fixture.Create<UserRole>();

        _sessionRepositoryMock
            .Setup(repository => repository.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _hashServiceMock
            .Setup(service => service.Verify(command.RefreshToken, session.RefreshTokenHash))
            .Returns(true);

        _userRepositoryMock
            .Setup(repository => repository.GetUserRoleAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ActiveSessionNotFound_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<RefreshCommand>();

        _sessionRepositoryMock
            .Setup(repository => repository.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().ContainSingle().Which.Value
            .Should().ContainSingle(SessionErrors.ActiveSessionForUserNotFound(command.UserId).Value);
        _hashServiceMock.Verify(
            service => service.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _userRepositoryMock.Verify(
            repository => repository.GetUserRoleAsync(command.UserId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenIncorrect_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<RefreshCommand>();
        var session = _fixture.Create<Session>();

        _sessionRepositoryMock
            .Setup(repository => repository.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _hashServiceMock
            .Setup(service => service.Verify(command.RefreshToken, session.RefreshTokenHash))
            .Returns(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().ContainSingle().Which.Value
            .Should().ContainSingle(SessionErrors.RefreshTokenForUserNotFound(command.RefreshToken, command.UserId).Value);
        _userRepositoryMock.Verify(
            repository => repository.GetUserRoleAsync(command.UserId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_UserNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<RefreshCommand>();
        var session = _fixture.Create<Session>();

        _sessionRepositoryMock
            .Setup(repository => repository.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _hashServiceMock
            .Setup(service => service.Verify(command.RefreshToken, session.RefreshTokenHash))
            .Returns(true);

        _userRepositoryMock
            .Setup(repository => repository.GetUserRoleAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRole?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlySessionRepository)
            .Returns(_sessionRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyUserRepository)
            .Returns(_userRepositoryMock.Object);

        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return unitOfWorkFactoryMock.Object;
    }
}
