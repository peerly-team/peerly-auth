using System;

namespace Peerly.Auth.Models.Email;

public sealed record EmailVerificationJobItem
{
    public required long Id { get; init; }
    public required string Token { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
}
