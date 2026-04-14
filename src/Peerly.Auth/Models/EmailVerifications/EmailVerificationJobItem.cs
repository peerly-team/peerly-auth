using System;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.EmailVerifications;

public sealed record EmailVerificationJobItem
{
    public required UserId UserId { get; init; }
    public required string Token { get; init; }
    public required string Email { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
}
