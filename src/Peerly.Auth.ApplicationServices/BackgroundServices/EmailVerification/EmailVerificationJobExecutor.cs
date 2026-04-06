using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Models;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Options;
using Peerly.Auth.Models.BackgroundService;
using Peerly.Auth.Models.Email;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification;

internal sealed class EmailVerificationJobExecutor : IExecutor<EmailVerificationJobItem>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ILogger<EmailVerificationJobExecutor> _logger;
    private readonly SmtpOptions _smtpOptions;

    public EmailVerificationJobExecutor(
        ILogger<EmailVerificationJobExecutor> logger,
        IOptions<SmtpOptions> smtpOptions,
        ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _unitOfWorkFactory = unitOfWorkFactory;
        _smtpOptions = smtpOptions.Value;
    }

    public async Task RunAsync(EmailVerificationJobItem jobItem, CancellationToken cancellationToken)
    {
        var mimeMessage = BuildMessage(jobItem);

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, ToSecureSocketOptions(_smtpOptions.SecurityMode), cancellationToken);

        if (_smtpOptions.Username is not null && _smtpOptions.Password is not null)
        {
            await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password, cancellationToken);
        }

        await client.SendAsync(mimeMessage, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);

        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);
        await unitOfWork.EmailVerificationRepository.UpdateAsync(
            jobItem.Id,
            builder => builder.Set(item => item.ProcessStatus, ProcessStatus.Done),
            cancellationToken);
    }

    private MimeMessage BuildMessage(EmailVerificationJobItem jobItem)
    {
        var verificationLink = $"{_smtpOptions.VerificationBaseUrl}?token={jobItem.Token}";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.SenderName, _smtpOptions.SenderEmail));
        message.To.Add(MailboxAddress.Parse(jobItem.Email));
        message.Subject = "Подтверждение электронной почты — Peerly";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $"""
                        <h2>Добро пожаловать в Peerly, {jobItem.Name}!</h2>
                        <p>Для подтверждения электронной почты перейдите по ссылке:</p>
                        <p><a href="{verificationLink}">{verificationLink}</a></p>
                        <p>Ссылка действительна до {jobItem.ExpirationTime:dd.MM.yyyy HH:mm} (МСК).</p>
                        <p>Если вы не регистрировались в Peerly, проигнорируйте это письмо.</p>
                        """,
            TextBody = $"""
                        Добро пожаловать в Peerly, {jobItem.Name}!

                        Для подтверждения электронной почты перейдите по ссылке:
                        {verificationLink}

                        Ссылка действительна до {jobItem.ExpirationTime:dd.MM.yyyy HH:mm} (МСК).

                        Если вы не регистрировались в Peerly, проигнорируйте это письмо.
                        """
        };

        message.Body = bodyBuilder.ToMessageBody();

        return message;
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
