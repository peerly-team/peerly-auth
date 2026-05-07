using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Abstractions;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Models;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Options;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification;

internal sealed class MailKitEmailSender : IEmailSender
{
    private readonly SmtpOptions _smtpOptions;

    public MailKitEmailSender(IOptions<SmtpOptions> smtpOptions)
    {
        _smtpOptions = smtpOptions.Value;
    }

    public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(
            _smtpOptions.Host,
            _smtpOptions.Port,
            ToSecureSocketOptions(_smtpOptions.SecurityMode),
            cancellationToken);

        if (_smtpOptions.Username is not null && _smtpOptions.Password is not null)
        {
            await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }

    private static SecureSocketOptions ToSecureSocketOptions(SmtpSecurityMode smtpSecurityMode)
    {
        return smtpSecurityMode switch
        {
            SmtpSecurityMode.None => SecureSocketOptions.None,
            SmtpSecurityMode.StartTls => SecureSocketOptions.StartTls,
            SmtpSecurityMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
            _ => throw new ArgumentOutOfRangeException(nameof(smtpSecurityMode), smtpSecurityMode, "Unsupported SMTP security mode")
        };
    }
}
