using System;
using System.Threading.Tasks;
using FluentAssertions;
using Peerly.Auth.Identifiers;
using Peerly.Auth.IntegrationTests.Infrastructure;
using Peerly.Auth.Models.EmailVerifications;
using Xunit;

namespace Peerly.Auth.IntegrationTests.BackgroundServices.EmailVerification;

public sealed class EmailVerificationJobIntegrationTests : EmailVerificationJobIntegrationTestBase
{
    public EmailVerificationJobIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task RunAsync_EmailSentSuccessfully_ShouldSetStatusToDone()
    {
        // Arrange
        var userId = await AddUserInDbAsync("success@test.com", "P@ssword123");
        var token = Guid.NewGuid().ToString("N");
        await AddEmailVerificationInDbAsync(userId, token);

        var jobItem = BuildJobItem(userId, token, "success@test.com");

        // Act
        await RunExecutorAsync(jobItem);

        // Assert
        var status = await GetEmailVerificationStatusAsync(userId);
        status.Should().Be("Done");
    }

    [Fact]
    public async Task RunAsync_EmailSentSuccessfully_ShouldSendEmailToCorrectRecipient()
    {
        // Arrange
        const string Email = "recipient@test.com";
        var userId = await AddUserInDbAsync(Email, "P@ssword123");
        var token = Guid.NewGuid().ToString("N");
        await AddEmailVerificationInDbAsync(userId, token);

        var jobItem = BuildJobItem(userId, token, Email);

        // Act
        await RunExecutorAsync(jobItem);

        // Assert
        EmailSender.SentMessages.Should().HaveCount(1);
        EmailSender.SentMessages.Should().AllSatisfy(msg =>
            msg.To.Mailboxes.Should().Contain(mailbox => mailbox.Address == Email));
    }

    [Fact]
    public async Task RunAsync_WhenSmtpFails_ShouldSetStatusToFailed()
    {
        // Arrange
        var userId = await AddUserInDbAsync("fail@test.com", "P@ssword123");
        var token = Guid.NewGuid().ToString("N");
        await AddEmailVerificationInDbAsync(userId, token);

        EmailSender.ShouldThrow = true;
        var jobItem = BuildJobItem(userId, token, "fail@test.com");

        // Act
        await RunExecutorAsync(jobItem);

        // Assert
        var status = await GetEmailVerificationStatusAsync(userId);
        status.Should().Be("Failed");
    }

    [Fact]
    public async Task RunAsync_WhenSmtpFails_ShouldIncrementFailCount()
    {
        // Arrange
        var userId = await AddUserInDbAsync("failcount@test.com", "P@ssword123");
        var token = Guid.NewGuid().ToString("N");
        await AddEmailVerificationInDbAsync(userId, token, failCount: 1);

        EmailSender.ShouldThrow = true;
        var jobItem = BuildJobItem(userId, token, "failcount@test.com");

        // Act
        await RunExecutorAsync(jobItem);

        // Assert
        var failCount = await GetEmailVerificationFailCountAsync(userId);
        failCount.Should().Be(2);
    }

    [Fact]
    public async Task RunAsync_WhenSmtpFails_ShouldStoreErrorMessage()
    {
        // Arrange
        var userId = await AddUserInDbAsync("error@test.com", "P@ssword123");
        var token = Guid.NewGuid().ToString("N");
        await AddEmailVerificationInDbAsync(userId, token);

        EmailSender.ShouldThrow = true;
        var jobItem = BuildJobItem(userId, token, "error@test.com");

        // Act
        await RunExecutorAsync(jobItem);

        // Assert
        var error = await GetEmailVerificationErrorAsync(userId);
        error.Should().Be("Simulated SMTP failure");
    }

    private static EmailVerificationJobItem BuildJobItem(long userId, string token, string email)
    {
        return new EmailVerificationJobItem
        {
            UserId = new UserId(userId),
            Token = token,
            Email = email,
            ExpirationTime = DateTimeOffset.UtcNow.AddHours(1)
        };
    }
}
