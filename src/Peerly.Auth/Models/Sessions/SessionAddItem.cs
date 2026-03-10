using System;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.Sessions;

public sealed record SessionAddItem
{
    public required UserId UserId { get; init; }
    public required string RefreshTokenHash { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
