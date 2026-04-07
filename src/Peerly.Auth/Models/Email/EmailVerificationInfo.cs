using System;

namespace Peerly.Auth.Models.Email;

public sealed record EmailVerificationInfo
{
    public required long Id { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
    public DateTimeOffset? VerificationTime { get; init; }
}
