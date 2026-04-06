using System;
using Peerly.Auth.Models.BackgroundService;

namespace Peerly.Auth.Models.Email;

public sealed record EmailVerificationUpdateItem
{
    public required ProcessStatus ProcessStatus { get; init; }
    public required string Error { get; init; }
    public required bool IncrementFailCount { get; init; }
    public required DateTimeOffset ProcessedTime { get; init; }
}
