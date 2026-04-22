using Peerly.Auth.ApplicationServices.Executors.Shared.Abstractions;

namespace Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Options;

internal sealed class EmailVerificationJobOptions : IMassExecutorOptions
{
    public const string SectionName = "EmailVerificationJob";

    public int MaxFailCount { get; set; }
    public int ProcessTimeoutSeconds { get; set; }
    public int MaxDegreeOfParallelism { get; set; }
    public int BatchSize { get; set; }
}
