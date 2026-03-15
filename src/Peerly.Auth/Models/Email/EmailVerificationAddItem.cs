using System;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.Email;

public sealed record EmailVerificationAddItem
{
    public required UserId UserId { get; init; }
    public required string TokenHash { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
}
