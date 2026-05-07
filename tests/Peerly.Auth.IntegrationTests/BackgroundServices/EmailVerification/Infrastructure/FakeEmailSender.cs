using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Abstractions;

namespace Peerly.Auth.IntegrationTests.BackgroundServices.EmailVerification.Infrastructure;

public sealed class FakeEmailSender : IEmailSender
{
    private readonly ConcurrentBag<MimeMessage> _sentMessages = [];

    public bool ShouldThrow { get; set; }

    public IReadOnlyCollection<MimeMessage> SentMessages => _sentMessages;

    public Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        if (ShouldThrow)
        {
            throw new Exception("Simulated SMTP failure");
        }

        _sentMessages.Add(message);
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _sentMessages.Clear();
        ShouldThrow = false;
    }
}
