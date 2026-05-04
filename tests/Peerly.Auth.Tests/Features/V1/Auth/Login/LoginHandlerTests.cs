using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;
using Peerly.Auth.Models.Auth;
using Peerly.Auth.Models.Sessions;
using Peerly.Auth.Models.User;
using Xunit;

namespace Peerly.Auth.Tests.Features.V1.Auth.Login;

public sealed class LoginHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ISessionRepository> _sessionRepositoryMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IHashService> _hashServiceMock = new();
    private readonly Mock<ILoginHandlerMapper> _mapperMock = new();
    private readonly Mock<ICommandValidator<LoginCommand, LoginCommandResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        SetupUnitOfWorkFactory();

        _handler = new LoginHandler(
            _tokenServiceMock.Object,
            _unitOfWorkFactoryMock.Object,
            _hashServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccessAndActiveSessionNotFound_ShouldCreateSession()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var user = _fixture.Create<User>();
        var authToken = _fixture.Create<AuthToken>();
        var refreshTokenHash = _fixture.Create<string>();
        var sessionAddItem = _fixture.Build<SessionAddItem>()
            .With(item => item.UserId, user.Id)
            .With(item => item.RefreshTokenHash, refreshTokenHash)
            .Create();

        SetupSuccessValidation(command);
        SetupUser(command, user);

        _sessionRepositoryMock
            .Setup(repository => repository.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        _tokenServiceMock
            .Setup(service => service.CreateAuthToken(user.Id, user.Role))
            .Returns(authToken);

        _hashServiceMock
            .Setup(service => service.HashAsync(authToken.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshTokenHash);

        _mapperMock
            .Setup(mapper => mapper.ToSessionAddItem(user.Id, refreshTokenHash))
            .Returns(sessionAddItem);

        _sessionRepositoryMock
            .Setup(repository => repository.AddAsync(sessionAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.UserId.Should().Be(user.Id);
        result.AsT0.AuthToken.Should().Be(authToken);

        _sessionRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<SessionFilter>(),
                It.IsAny<SessionUpdateItem>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _sessionRepositoryMock.Verify(
            repository => repository.AddAsync(sessionAddItem, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccessAndActiveSessionExists_ShouldCancelPreviousSessionAndCreateSession()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var user = _fixture.Create<User>();
        var session = _fixture.Create<Session>();
        var sessionUpdateItem = _fixture.Create<SessionUpdateItem>();
        var authToken = _fixture.Create<AuthToken>();
        var refreshTokenHash = _fixture.Create<string>();
        var sessionAddItem = _fixture.Build<SessionAddItem>()
            .With(item => item.UserId, user.Id)
            .With(item => item.RefreshTokenHash, refreshTokenHash)
            .Create();
        var expectedSessionFilter = LoginHandlerMapper.ToSessionFilter(session);

        SetupSuccessValidation(command);
        SetupUser(command, user);

        _sessionRepositoryMock
            .Setup(repository => repository.GetAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mapperMock
            .Setup(mapper => mapper.ToSessionUpdateItem())
            .Returns(sessionUpdateItem);

        _sessionRepositoryMock
            .Setup(repository => repository.UpdateAsync(expectedSessionFilter, sessionUpdateItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(service => service.CreateAuthToken(user.Id, user.Role))
            .Returns(authToken);

        _hashServiceMock
            .Setup(service => service.HashAsync(authToken.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshTokenHash);

        _mapperMock
            .Setup(mapper => mapper.ToSessionAddItem(user.Id, refreshTokenHash))
            .Returns(sessionAddItem);

        _sessionRepositoryMock
            .Setup(repository => repository.AddAsync(sessionAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();

        _sessionRepositoryMock.Verify(
            repository => repository.UpdateAsync(expectedSessionFilter, sessionUpdateItem, It.IsAny<CancellationToken>()),
            Times.Once);
        _sessionRepositoryMock.Verify(
            repository => repository.AddAsync(sessionAddItem, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationError_ShouldReturnValidationError()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
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
            factory => factory.CreateAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_OtherError_ShouldReturnOtherError()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
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
            factory => factory.CreateAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupUnitOfWorkFactory()
    {
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.UserRepository)
            .Returns(_userRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.SessionRepository)
            .Returns(_sessionRepositoryMock.Object);

        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }

    private void SetupSuccessValidation(LoginCommand command)
    {
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());
    }

    private void SetupUser(LoginCommand command, User user)
    {
        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }
}
