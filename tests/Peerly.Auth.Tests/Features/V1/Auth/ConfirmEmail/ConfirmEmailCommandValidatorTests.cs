using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.ConfirmEmail;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Validation.Errors;
using Peerly.Auth.Models.EmailVerifications;
using Xunit;

namespace Peerly.Auth.Tests.Features.V1.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandValidatorTests
{
    private readonly Mock<ICommonReadOnlyUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReadOnlyEmailVerificationRepository> _emailVerificationRepositoryMock = new();
    private readonly Mock<IClock> _clockMock = new();

    private readonly Fixture _fixture = new();
    private readonly ConfirmEmailCommandValidator _validator;
    private readonly DateTimeOffset _currentTime;

    public ConfirmEmailCommandValidatorTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();

        _currentTime = _fixture.Create<DateTimeOffset>();
        _clockMock
            .Setup(clock => clock.GetCurrentTime())
            .Returns(_currentTime);

        _validator = new ConfirmEmailCommandValidator(unitOfWorkFactory, _clockMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_TokenExistsAndNotExpired_ShouldSuccess()
    {
        // Arrange
        var command = _fixture.Create<ConfirmEmailCommand>();
        var userExpirationTime = _fixture.Build<UserExpirationTime>()
            .With(item => item.ExpirationTime, _currentTime.AddMinutes(1))
            .Create();

        _emailVerificationRepositoryMock
            .Setup(repository => repository.GetUserExpirationTimeByTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userExpirationTime);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_TokenNotFound_ShouldBeOtherErrorNotFound()
    {
        // Arrange
        var command = _fixture.Create<ConfirmEmailCommand>();

        _emailVerificationRepositoryMock
            .Setup(repository => repository.GetUserExpirationTimeByTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserExpirationTime?)null);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ValidateAsync_TokenExpired_ShouldBeValidationError()
    {
        // Arrange
        var command = new ConfirmEmailCommand { Token = _fixture.Create<string>() };
        var userExpirationTime = _fixture.Build<UserExpirationTime>()
            .With(item => item.ExpirationTime, _currentTime.AddMinutes(-1))
            .Create();

        _emailVerificationRepositoryMock
            .Setup(repository => repository.GetUserExpirationTimeByTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userExpirationTime);

        // Act
        var result = await _validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Errors.Should().ContainSingle().Which.Value.Should().ContainSingle(EmailVerificationErrors.TokenExpired.Value);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.ReadOnlyEmailVerificationRepository)
            .Returns(_emailVerificationRepositoryMock.Object);

        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateReadOnlyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return unitOfWorkFactoryMock.Object;
    }
}
