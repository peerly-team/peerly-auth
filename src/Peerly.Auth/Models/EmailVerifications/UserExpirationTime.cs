using System;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.EmailVerifications;

public sealed record UserExpirationTime
{
    public required UserId UserId { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
}
