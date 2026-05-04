using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.ConfirmEmail;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;
using Xunit;

namespace Peerly.Auth.Tests.Features.V1.Auth.ConfirmEmail;

public sealed class ConfirmEmailHandlerTests
{
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmailVerificationRepository> _emailVerificationRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ICommandValidator<ConfirmEmailCommand, Success>> _validatorMock = new();

    private readonly Fixture _fixture = new();
    private readonly ConfirmEmailHandler _handler;

    public ConfirmEmailHandlerTests()
    {
        var unitOfWorkFactory = SetupUnitOfWorkFactory();
        _handler = new ConfirmEmailHandler(unitOfWorkFactory, _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationResultSuccess_ShouldConfirmUserEmail()
    {
        // Arrange
        var command = _fixture.Create<ConfirmEmailCommand>();
        var userId = _fixture.Create<UserId>();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandValidationResult.Ok());

        _emailVerificationRepositoryMock
            .Setup(repository => repository.GetUserIdByTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _userRepositoryMock
            .Setup(repository => repository.UpdateAsync(
                userId,
                It.Is<Action<IUpdateBuilder<UserUpdateItem>>>(configureUpdate => ConfiguresIsConfirmedTrue(configureUpdate)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT0.Should().BeTrue();
        _userRepositoryMock.Verify(
            repository => repository.UpdateAsync(
                userId,
                It.Is<Action<IUpdateBuilder<UserUpdateItem>>>(configureUpdate => ConfiguresIsConfirmedTrue(configureUpdate)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationError_ShouldReturnValidationError()
    {
        // Arrange
        var command = _fixture.Create<ConfirmEmailCommand>();
        var validationError = ValidationError.From(_fixture.Create<string>());

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationError);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Should().Be(validationError);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.UserRepository,
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_OtherError_ShouldReturnOtherError()
    {
        // Arrange
        var command = _fixture.Create<ConfirmEmailCommand>();
        var otherError = OtherError.NotFound();

        _validatorMock
            .Setup(validator => validator.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherError);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Should().Be(otherError);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.UserRepository,
            Times.Never);
    }

    private ICommonUnitOfWorkFactory SetupUnitOfWorkFactory()
    {
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.EmailVerificationRepository)
            .Returns(_emailVerificationRepositoryMock.Object);
        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.UserRepository)
            .Returns(_userRepositoryMock.Object);

        var unitOfWorkFactoryMock = new Mock<ICommonUnitOfWorkFactory>();
        unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        return unitOfWorkFactoryMock.Object;
    }

    private static bool ConfiguresIsConfirmedTrue(Action<IUpdateBuilder<UserUpdateItem>> configureUpdate)
    {
        var builder = new CapturingUpdateBuilder<UserUpdateItem>();
        configureUpdate(builder);

        return builder.Values.TryGetValue(nameof(UserUpdateItem.IsConfirmed), out var value)
            && value is true;
    }

    private sealed class CapturingUpdateBuilder<TItem> : IUpdateBuilder<TItem>
    {
        public Dictionary<string, object?> Values { get; } = [];

        public IUpdateBuilder<TItem> Set<TProperty>(
            Expression<Func<TItem, TProperty>> propertyExpression,
            TProperty propertyValue)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
            {
                Values[memberExpression.Member.Name] = propertyValue;
            }

            return this;
        }
    }
}
