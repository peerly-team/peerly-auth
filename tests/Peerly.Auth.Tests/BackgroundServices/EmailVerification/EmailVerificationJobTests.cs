using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Options;
using Peerly.Auth.Models.BackgroundService;
using Peerly.Auth.Models.EmailVerifications;
using Quartz;
using Xunit;

namespace Peerly.Auth.Tests.BackgroundServices.EmailVerification;

public sealed class EmailVerificationJobTests
{
    private readonly Mock<ICommonUnitOfWorkFactory> _unitOfWorkFactoryMock = new();
    private readonly Mock<ICommonUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmailVerificationRepository> _emailVerificationRepositoryMock = new();
    private readonly Mock<IMassExecutor<EmailVerificationJobItem>> _executorMock = new();
    private readonly Mock<IJobExecutionContext> _jobContextMock = new();

    private readonly Fixture _fixture = new();
    private readonly EmailVerificationJob _job;

    public EmailVerificationJobTests()
    {
        _unitOfWorkMock
            .SetupGet(uow => uow.EmailVerificationRepository)
            .Returns(_emailVerificationRepositoryMock.Object);

        _unitOfWorkFactoryMock
            .Setup(factory => factory.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _jobContextMock
            .SetupGet(ctx => ctx.CancellationToken)
            .Returns(CancellationToken.None);

        _job = new EmailVerificationJob(
            _unitOfWorkFactoryMock.Object,
            Options.Create(new EmailVerificationJobOptions
            {
                MaxFailCount = 3,
                ProcessTimeoutSeconds = 300,
                MaxDegreeOfParallelism = 2,
                BatchSize = 10
            }),
            new Mock<Microsoft.Extensions.Logging.ILogger<EmailVerificationJob>>().Object,
            _executorMock.Object);
    }

    [Fact]
    public async Task Execute_ItemsAvailable_ShouldDelegateTakenItemsToExecutor()
    {
        // Arrange
        var expectedItems = _fixture.CreateMany<EmailVerificationJobItem>(2).ToList();

        _emailVerificationRepositoryMock
            .Setup(repo => repo.TakeAsync(It.IsAny<EmailVerificationFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _executorMock.Verify(
            executor => executor.RunAsync(
                It.Is<IReadOnlyCollection<EmailVerificationJobItem>>(items => items.SequenceEqual(expectedItems)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_NoItemsAvailable_ShouldCallExecutorWithEmptyCollection()
    {
        // Arrange
        _emailVerificationRepositoryMock
            .Setup(repo => repo.TakeAsync(It.IsAny<EmailVerificationFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<EmailVerificationJobItem>());

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _executorMock.Verify(
            executor => executor.RunAsync(
                It.Is<IReadOnlyCollection<EmailVerificationJobItem>>(items => items.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldBuildFilterFromOptionsSettings()
    {
        // Arrange
        var job = new EmailVerificationJob(
            _unitOfWorkFactoryMock.Object,
            Options.Create(new EmailVerificationJobOptions
            {
                MaxFailCount = 5,
                ProcessTimeoutSeconds = 120,
                MaxDegreeOfParallelism = 2,
                BatchSize = 50
            }),
            new Mock<Microsoft.Extensions.Logging.ILogger<EmailVerificationJob>>().Object,
            _executorMock.Object);

        _emailVerificationRepositoryMock
            .Setup(repo => repo.TakeAsync(It.IsAny<EmailVerificationFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<EmailVerificationJobItem>());

        // Act
        await job.Execute(_jobContextMock.Object);

        // Assert
        _emailVerificationRepositoryMock.Verify(
            repo => repo.TakeAsync(
                It.Is<EmailVerificationFilter>(f =>
                    f.MaxFailCount == 5 &&
                    f.Limit == 50 &&
                    f.ProcessTimeoutSeconds == TimeSpan.FromSeconds(120) &&
                    f.ProcessStatuses.Contains(ProcessStatus.Created) &&
                    f.ProcessStatuses.Contains(ProcessStatus.InProgress) &&
                    f.ProcessStatuses.Contains(ProcessStatus.Failed)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_RepositoryThrows_ShouldNotPropagate()
    {
        // Arrange
        _emailVerificationRepositoryMock
            .Setup(repo => repo.TakeAsync(It.IsAny<EmailVerificationFilter>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> act = () => _job.Execute(_jobContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
        _executorMock.Verify(
            executor => executor.RunAsync(It.IsAny<IReadOnlyCollection<EmailVerificationJobItem>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_ExecutorThrows_ShouldNotPropagate()
    {
        // Arrange
        _emailVerificationRepositoryMock
            .Setup(repo => repo.TakeAsync(It.IsAny<EmailVerificationFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<EmailVerificationJobItem>());

        _executorMock
            .Setup(executor => executor.RunAsync(It.IsAny<IReadOnlyCollection<EmailVerificationJobItem>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Executor error"));

        // Act
        Func<Task> act = () => _job.Execute(_jobContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
