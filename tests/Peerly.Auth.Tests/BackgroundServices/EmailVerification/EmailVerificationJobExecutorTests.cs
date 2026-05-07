using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Abstractions;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Options;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.BackgroundService;
using Peerly.Auth.Models.EmailVerifications;
using Xunit;

namespace Peerly.Auth.Tests.BackgroundServices.EmailVerification;

public sealed class EmailVerificationJobExecutorTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmailVerificationRepository> _emailVerificationRepositoryMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();

    private readonly Fixture _fixture = new();
    private readonly EmailVerificationJobExecutor _executor;

    public EmailVerificationJobExecutorTests()
    {
        _unitOfWorkMock
            .SetupGet(uow => uow.EmailVerificationRepository)
            .Returns(_emailVerificationRepositoryMock.Object);

        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _executor = new EmailVerificationJobExecutor(
            Mock.Of<ILogger<EmailVerificationJobExecutor>>(),
            _emailSenderMock.Object,
            Options.Create(new SmtpOptions
            {
                Host = "localhost",
                Port = 1025,
                SenderEmail = "test@peerly.app",
                SenderName = "Peerly",
                VerificationBaseUrl = "http://localhost/confirm"
            }),
            _unitOfWorkFactoryMock.Object);
    }

    [Fact]
    public async Task RunAsync_EmailSentSuccessfully_ShouldUpdateStatusToDone()
    {
        // Arrange
        var jobItem = CreateJobItem();

        _emailVerificationRepositoryMock
            .Setup(repo => repo.UpdateAsync(jobItem.UserId, It.IsAny<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _executor.RunAsync(jobItem, CancellationToken.None);

        // Assert
        _emailVerificationRepositoryMock.Verify(
            repo => repo.UpdateAsync(
                jobItem.UserId,
                It.Is<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(configure => ConfiguresStatus(configure, ProcessStatus.Done)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_EmailSentSuccessfully_ShouldNotCallFailureUpdate()
    {
        // Arrange
        var jobItem = CreateJobItem();

        _emailVerificationRepositoryMock
            .Setup(repo => repo.UpdateAsync(jobItem.UserId, It.IsAny<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _executor.RunAsync(jobItem, CancellationToken.None);

        // Assert
        _emailVerificationRepositoryMock.Verify(
            repo => repo.UpdateAsync(
                jobItem.UserId,
                It.Is<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(configure => ConfiguresStatus(configure, ProcessStatus.Failed)),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_EmailSendingFails_ShouldUpdateStatusToFailed()
    {
        // Arrange
        var jobItem = CreateJobItem();

        _emailSenderMock
            .Setup(sender => sender.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("smtp error"));

        _emailVerificationRepositoryMock
            .Setup(repo => repo.UpdateAsync(jobItem.UserId, It.IsAny<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _executor.RunAsync(jobItem, CancellationToken.None);

        // Assert
        _emailVerificationRepositoryMock.Verify(
            repo => repo.UpdateAsync(
                jobItem.UserId,
                It.Is<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(configure => ConfiguresStatus(configure, ProcessStatus.Failed)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_EmailSendingFails_ShouldIncrementFailCount()
    {
        // Arrange
        var jobItem = CreateJobItem();

        _emailSenderMock
            .Setup(sender => sender.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("smtp error"));

        _emailVerificationRepositoryMock
            .Setup(repo => repo.UpdateAsync(jobItem.UserId, It.IsAny<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _executor.RunAsync(jobItem, CancellationToken.None);

        // Assert
        _emailVerificationRepositoryMock.Verify(
            repo => repo.UpdateAsync(
                jobItem.UserId,
                It.Is<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(configure => ConfiguresIncrementFailCount(configure)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_EmailSendingFails_ShouldStoreErrorMessage()
    {
        // Arrange
        var jobItem = CreateJobItem();
        const string ErrorMessage = "smtp error";

        _emailSenderMock
            .Setup(sender => sender.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(ErrorMessage));

        _emailVerificationRepositoryMock
            .Setup(repo => repo.UpdateAsync(jobItem.UserId, It.IsAny<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _executor.RunAsync(jobItem, CancellationToken.None);

        // Assert
        _emailVerificationRepositoryMock.Verify(
            repo => repo.UpdateAsync(
                jobItem.UserId,
                It.Is<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(configure => ConfiguresError(configure, ErrorMessage)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldSendToCorrectRecipient()
    {
        // Arrange
        const string RecipientEmail = "user@test.com";
        var jobItem = new EmailVerificationJobItem
        {
            UserId = _fixture.Create<UserId>(),
            Token = _fixture.Create<string>(),
            Email = RecipientEmail,
            ExpirationTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        MimeMessage? capturedMessage = null;
        _emailSenderMock
            .Setup(sender => sender.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()))
            .Callback<MimeMessage, CancellationToken>((msg, _) => capturedMessage = msg);

        _emailVerificationRepositoryMock
            .Setup(repo => repo.UpdateAsync(jobItem.UserId, It.IsAny<Action<IUpdateBuilder<EmailVerificationUpdateItem>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _executor.RunAsync(jobItem, CancellationToken.None);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.To.Mailboxes.Should().Contain(mailbox => mailbox.Address == RecipientEmail);
    }

    private EmailVerificationJobItem CreateJobItem()
    {
        return new EmailVerificationJobItem
        {
            UserId = _fixture.Create<UserId>(),
            Token = _fixture.Create<string>(),
            Email = "test@example.com",
            ExpirationTime = DateTimeOffset.UtcNow.AddHours(1)
        };
    }

    private static bool ConfiguresStatus(
        Action<IUpdateBuilder<EmailVerificationUpdateItem>> configure,
        ProcessStatus expectedStatus)
    {
        var builder = new CapturingUpdateBuilder<EmailVerificationUpdateItem>();
        configure(builder);
        return builder.Values.TryGetValue(nameof(EmailVerificationUpdateItem.ProcessStatus), out var value)
            && value is ProcessStatus status
            && status == expectedStatus;
    }

    private static bool ConfiguresIncrementFailCount(Action<IUpdateBuilder<EmailVerificationUpdateItem>> configure)
    {
        var builder = new CapturingUpdateBuilder<EmailVerificationUpdateItem>();
        configure(builder);
        return builder.Values.TryGetValue(nameof(EmailVerificationUpdateItem.IncrementFailCount), out var value)
            && value is true;
    }

    private static bool ConfiguresError(
        Action<IUpdateBuilder<EmailVerificationUpdateItem>> configure,
        string expectedError)
    {
        var builder = new CapturingUpdateBuilder<EmailVerificationUpdateItem>();
        configure(builder);
        return builder.Values.TryGetValue(nameof(EmailVerificationUpdateItem.Error), out var value)
            && value is string error
            && error == expectedError;
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
