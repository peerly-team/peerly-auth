using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Options;
using Peerly.Auth.ApplicationServices.Executors.Shared;
using Peerly.Auth.Models.EmailVerifications;
using Peerly.Auth.Tools.Abstractions;
using Quartz;

namespace Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification;

[ExcludeFromCodeCoverage]
internal sealed class EmailVerificationJobInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services
            .AddOptions<EmailVerificationJobOptions>()
            .BindConfiguration(EmailVerificationJobOptions.SectionName);

        services
            .AddOptions<SmtpOptions>()
            .BindConfiguration(SmtpOptions.SectionName);

        services
            .AddScoped<IMassExecutor<EmailVerificationJobItem>,
                ConcurrentMassExecutorAdapter<EmailVerificationJobItem, EmailVerificationJobOptions>>()
            .AddScoped<IExecutor<EmailVerificationJobItem>, EmailVerificationJobExecutor>();

        services.AddQuartz(
            q =>
            {
                var jobKey = new JobKey(nameof(EmailVerificationJob));

                q.AddJob<EmailVerificationJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(
                    opts => opts
                        .ForJob(jobKey)
                        .WithIdentity($"{nameof(EmailVerificationJob)}-trigger")
                        .WithCronSchedule("0/15 * * * * ?"));
            });

        services.AddQuartzHostedService(
            opts =>
            {
                opts.WaitForJobsToComplete = true;
            });
    }
}
