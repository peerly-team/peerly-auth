using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;
using Xunit;

namespace Peerly.Auth.Tests.Features.V1.Auth.Register;

public sealed class RegisterCommandValidatorTests
{
    private const string StrongPassword = "CorrectHorseBatteryStaple123!";
    private const string WeakPassword = "123";

    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyUserRepository> _userRepositoryMock = new();

    private readonly Fixture _fixture = new();
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _validator = new RegisterCommandValidator(unitOfWorkFactory);
    }

    [Fact]
    public async Task ValidateAsync_EmailNotExistsAndPasswordStrong_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Build<RegisterCommand>()
            .With(command => command.Password, StrongPassword)
            .Create();

        _userRepositoryMock
            .Setup(repository => repository.ExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_EmailExists_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Build<RegisterCommand>()
            .With(command => command.Password, StrongPassword)
            .Create();

        _userRepositoryMock
            .Setup(repository => repository.ExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().ContainSingle().Which.Value
            .Should().ContainSingle(EmailErrors.EmailAlreadyUsed.Value);
    }

    [Fact]
    public async Task ValidateAsync_PasswordTooSimple_ShouldBeValidationError()
    {
        // Arrange
        var command = _fixture.Build<RegisterCommand>()
            .With(command => command.Password, WeakPassword)
            .Create();

        _userRepositoryMock
            .Setup(repository => repository.ExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().ContainSingle().Which.Value
            .Should().ContainSingle(PasswordErrors.IsTooSimple.Value);
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
