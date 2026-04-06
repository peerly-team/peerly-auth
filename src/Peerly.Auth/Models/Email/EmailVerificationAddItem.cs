using System;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.BackgroundService;

namespace Peerly.Auth.Models.Email;

public sealed record EmailVerificationAddItem
{
    public required UserId UserId { get; init; }
    public required string Token { get; init; }
    public required ProcessStatus ProcessStatus { get; init; }
    public required int FailCount { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
}
