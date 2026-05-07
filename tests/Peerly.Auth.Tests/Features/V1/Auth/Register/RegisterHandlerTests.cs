using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Auth;
using Peerly.Auth.Models.EmailVerifications;
using Peerly.Auth.Models.Outbox;
using Peerly.Auth.Models.Sessions;
using Peerly.Auth.Models.User;
using Xunit;

namespace Peerly.Auth.Tests.Features.V1.Auth.Register;

public sealed class RegisterHandlerTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IOperationSet> _operationSetMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IEmailVerificationRepository> _emailVerificationRepositoryMock = new();
    private readonly Mock<ISessionRepository> _sessionRepositoryMock = new();
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IHashService> _hashServiceMock = new();
    private readonly Mock<IRegisterHandlerMapper> _mapperMock = new();
    private readonly Mock<ICommandValidator<RegisterCommand, RegisterCommandResponse>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        SetupUnitOfWorkFactory();

        _handler = new RegisterHandler(
            _unitOfWorkFactoryMock.Object,
            _tokenServiceMock.Object,
            _hashServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldCreateUserEmailVerificationSessionAndOutboxMessage()
    {
        // Arrange
        var command = _fixture.Create<RegisterCommand>();
        var userId = _fixture.Create<UserId>();
        var emailVerificationToken = _fixture.Create<string>();
        var passwordHash = _fixture.Create<string>();
        var refreshTokenHash = _fixture.Create<string>();
        var authToken = _fixture.Create<AuthToken>();
        var userAddItem = _fixture.Create<UserAddItem>();
        var emailVerificationAddItem = _fixture.Create<EmailVerificationAddItem>();
        var sessionAddItem = _fixture.Create<SessionAddItem>();
        var outboxMessageAddItem = _fixture.Create<OutboxMessageAddItem>();

        SetupSuccessValidation(command);

        _tokenServiceMock
            .Setup(service => service.CreateRefreshToken())
            .Returns(emailVerificationToken);

        _hashServiceMock
            .Setup(service => service.HashAsync(command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordHash);

        _mapperMock
            .Setup(mapper => mapper.ToUserAddItem(command, passwordHash))
            .Returns(userAddItem);

        _userRepositoryMock
            .Setup(repository => repository.AddAsync(userAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _mapperMock
            .Setup(mapper => mapper.ToEmailVerificationAddItem(userId, emailVerificationToken))
            .Returns(emailVerificationAddItem);

        _emailVerificationRepositoryMock
            .Setup(repository => repository.AddAsync(emailVerificationAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(service => service.CreateAuthToken(userId, command.Role))
            .Returns(authToken);

        _hashServiceMock
            .Setup(service => service.HashAsync(authToken.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshTokenHash);

        _mapperMock
            .Setup(mapper => mapper.ToSessionAddItem(userId, refreshTokenHash))
            .Returns(sessionAddItem);

        _sessionRepositoryMock
            .Setup(repository => repository.AddAsync(sessionAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mapperMock
            .Setup(mapper => mapper.ToOutboxMessage(userId, command))
            .Returns(outboxMessageAddItem);

        _outboxRepositoryMock
            .Setup(repository => repository.AddAsync(outboxMessageAddItem, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _operationSetMock
            .Setup(operationSet => operationSet.Complete(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.UserId.Should().Be(userId);
        result.AsT0.AuthToken.Should().Be(authToken);

        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.StartOperationSet(It.IsAny<CancellationToken>()), Times.Once);
        _operationSetMock.Verify(operationSet => operationSet.Complete(It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repository => repository.AddAsync(userAddItem, It.IsAny<CancellationToken>()), Times.Once);
        _emailVerificationRepositoryMock.Verify(repository => repository.AddAsync(emailVerificationAddItem, It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepositoryMock.Verify(repository => repository.AddAsync(sessionAddItem, It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(repository => repository.AddAsync(outboxMessageAddItem, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationError_ShouldReturnValidationError()
    {
        // Arrange
        var command = _fixture.Create<RegisterCommand>();
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
        var command = _fixture.Create<RegisterCommand>();
        var otherError = OtherError.Conflict();

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
            .SetupGet(unitOfWork => unitOfWork.EmailVerificationRepository)
            .Returns(_emailVerificationRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.SessionRepository)
            .Returns(_sessionRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.OutboxRepository)
            .Returns(_outboxRepositoryMock.Object);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.StartOperationSet(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_operationSetMock.Object);

        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);
    }

    private void SetupSuccessValidation(RegisterCommand command)
    {
        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());
    }
}
