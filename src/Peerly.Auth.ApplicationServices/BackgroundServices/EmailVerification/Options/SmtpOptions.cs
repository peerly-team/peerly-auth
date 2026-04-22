using Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Models;

namespace Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Options;

internal sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string SenderEmail { get; init; }
    public required string SenderName { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public SmtpSecurityMode SecurityMode { get; init; }
    public required string VerificationBaseUrl { get; init; }
}
