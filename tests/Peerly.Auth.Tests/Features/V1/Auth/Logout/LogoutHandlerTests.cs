using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.Models.Sessions;
using Xunit;

namespace Peerly.Auth.Tests.Features.V1.Auth.Logout;

public sealed class LogoutHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISessionRepository> _sessionRepositoryMock = new();
    private readonly Mock<IHashService> _hashServiceMock = new();
    private readonly Mock<IClock> _clockMock = new();

    private readonly Fixture _fixture = new();
    private readonly LogoutHandler _handler;
    private readonly DateTimeOffset _currentTime;

    public LogoutHandlerTests()
    {
        SetupUnitOfWorkFactory();

        _handler = new LogoutHandler(
            _unitOfWorkFactoryMock.Object,
            _hashServiceMock.Object,
            _clockMock.Object);

        _currentTime = _fixture.Create<DateTimeOffset>();
        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(_currentTime);
    }

    [Fact]
    public async Task ExecuteAsync_ActiveSessionExistsAndRefreshTokenCorrect_ShouldCancelSession()
    {
        // Arrange
        var command = _fixture.Create<LogoutCommand>();
        var session = _fixture.Create<Session>();
        var expectedSessionFilter = session.ToSessionFilter();
        var expectedSessionUpdateItem = LogoutHandlerMapper.ToSessionUpdateItem(_currentTime);

        _sessionRepositoryMock
            .Setup(repository => repository.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _hashServiceMock
            .Setup(service => service.Verify(command.RefreshToken, session.RefreshTokenHash))
            .Returns(true);

        _sessionRepositoryMock
            .Setup(repository => repository.UpdateAsync(
                expectedSessionFilter,
                expectedSessionUpdateItem,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
        _sessionRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                expectedSessionFilter,
                expectedSessionUpdateItem,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ActiveSessionNotFound_ShouldReturnValidationError()
    {
        // Arrange
        var command = _fixture.Create<LogoutCommand>();

        _sessionRepositoryMock
            .Setup(repository => repository.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().ContainSingle().Which.Value
            .Should().ContainSingle(SessionErrors.ActiveSessionForUserNotFound(command.UserId).Value);
        _hashServiceMock.Verify(
            service => service.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _sessionRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<SessionFilter>(),
                It.IsAny<SessionUpdateItem>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_RefreshTokenIncorrect_ShouldReturnValidationError()
    {
        // Arrange
        var command = _fixture.Create<LogoutCommand>();
        var session = _fixture.Create<Session>();

        _sessionRepositoryMock
            .Setup(repository => repository.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _hashServiceMock
            .Setup(service => service.Verify(command.RefreshToken, session.RefreshTokenHash))
            .Returns(false);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().ContainSingle().Which.Value
            .Should().ContainSingle(SessionErrors.RefreshTokenForUserNotFound(command.RefreshToken, command.UserId).Value);
        _sessionRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<SessionFilter>(),
                It.IsAny<SessionUpdateItem>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.SessionRepository)
            .Returns(_sessionRepositoryMock.Object);

        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }
}
