using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Abstractions;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Options;
using Peerly.Auth.Models.BackgroundService;
using Peerly.Auth.Models.EmailVerifications;

namespace Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification;

internal sealed class EmailVerificationJobExecutor : IExecutor<EmailVerificationJobItem>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailVerificationJobExecutor> _logger;
    private readonly SmtpOptions _smtpOptions;

    public EmailVerificationJobExecutor(
        ILogger<EmailVerificationJobExecutor> logger,
        IEmailSender emailSender,
        IOptions<SmtpOptions> smtpOptions,
        ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _emailSender = emailSender;
        _smtpOptions = smtpOptions.Value;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task RunAsync(EmailVerificationJobItem jobItem, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "{Job} | Processing started | UserId: {UserId}",
            nameof(EmailVerificationJob),
            jobItem.UserId);

        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        try
        {
            var mimeMessage = BuildMessage(jobItem);
            await _emailSender.SendAsync(mimeMessage, cancellationToken);

            await unitOfWork.EmailVerificationRepository.UpdateAsync(
                jobItem.UserId,
                builder => builder.Set(item => item.ProcessStatus, ProcessStatus.Done),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{Job} | An error occurred | UserId: {UserId} | Error message: {ErrorMessage}",
                nameof(EmailVerificationJob),
                jobItem.UserId,
                ex.Message);

            await unitOfWork.EmailVerificationRepository.UpdateAsync(
                jobItem.UserId,
                builder => builder
                    .Set(item => item.ProcessStatus, ProcessStatus.Failed)
                    .Set(item => item.IncrementFailCount, true)
                    .Set(item => item.Error, ex.Message),
                cancellationToken);
        }

        _logger.LogInformation(
            "{Job} | Processing completed | UserId: {UserId}",
            nameof(EmailVerificationJob),
            jobItem.UserId);
    }

    private MimeMessage BuildMessage(EmailVerificationJobItem jobItem)
    {
        var verificationLink = $"{_smtpOptions.VerificationBaseUrl}?Token={jobItem.Token}";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.SenderName, _smtpOptions.SenderEmail));
        message.To.Add(MailboxAddress.Parse(jobItem.Email));
        message.Subject = "Подтверждение электронной почты — Peerly";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $"""
                        <h2>Добро пожаловать в Peerly!</h2>
                        <p>Для подтверждения электронной почты перейдите по ссылке:</p>
                        <p><a href="{verificationLink}">{verificationLink}</a></p>
                        <p>Ссылка действительна до {jobItem.ExpirationTime:dd.MM.yyyy HH:mm} (МСК).</p>
                        <p>Если вы не регистрировались в Peerly, проигнорируйте это письмо.</p>
                        """,
            TextBody = $"""
                        Добро пожаловать в Peerly!

                        Для подтверждения электронной почты перейдите по ссылке:
                        {verificationLink}

                        Ссылка действительна до {jobItem.ExpirationTime:dd.MM.yyyy HH:mm} (МСК).

                        Если вы не регистрировались в Peerly, проигнорируйте это письмо.
                        """
        };

        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }
}
