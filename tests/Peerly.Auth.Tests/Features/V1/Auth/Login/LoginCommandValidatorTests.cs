using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.Models.User;
using Xunit;

namespace Peerly.Auth.Tests.Features.V1.Auth.Login;

public sealed class LoginCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyUserRepository> _userRepositoryMock = new();
    private readonly Mock<IHashService> _hashServiceMock = new();

    private readonly Fixture _fixture = new();
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new LoginCommandValidator(unitOfWorkFactory, _hashServiceMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_UserExistsAndPasswordCorrect_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var user = _fixture.Create<User>();

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hashServiceMock
            .Setup(service => service.Verify(command.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_UserNotFound_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().ContainSingle().Which.Value.Should().ContainSingle(EmailErrors.NotFound.Value);
        _hashServiceMock.Verify(
            service => service.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_PasswordIncorrect_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var user = _fixture.Create<User>();

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hashServiceMock
            .Setup(service => service.Verify(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().ContainSingle().Which.Value.Should().ContainSingle(PasswordErrors.Incorrect.Value);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
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
