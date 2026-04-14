using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Options;
using Peerly.Auth.Models.BackgroundService;
using Peerly.Auth.Models.EmailVerifications;
using Quartz;

namespace Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification;

[DisallowConcurrentExecution]
internal sealed class EmailVerificationJob : IJob
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMassExecutor<EmailVerificationJobItem> _executor;
    private readonly EmailVerificationJobOptions _options;
    private readonly ILogger<EmailVerificationJob> _logger;

    public EmailVerificationJob(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        IOptions<EmailVerificationJobOptions> options,
        ILogger<EmailVerificationJob> logger,
        IMassExecutor<EmailVerificationJobItem> executor)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _options = options.Value;
        _logger = logger;
        _executor = executor;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(context.CancellationToken);

            var filter = GetEmailVerificationFilter();
            var jobItems = await unitOfWork.EmailVerificationRepository.TakeAsync(filter, context.CancellationToken);

            await _executor.RunAsync(jobItems, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{Job} | An unexpected error occurred | Error message: {ErrorMessage}",
                nameof(EmailVerificationJob),
                ex.Message);
        }
    }

    private EmailVerificationFilter GetEmailVerificationFilter()
    {
        return new EmailVerificationFilter
        {
            ProcessStatuses = [ProcessStatus.Created, ProcessStatus.InProgress, ProcessStatus.Failed],
            MaxFailCount = _options.MaxFailCount,
            ProcessTimeoutSeconds = TimeSpan.FromSeconds(_options.ProcessTimeoutSeconds),
            Limit = _options.BatchSize
        };
    }
}
