using System;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.User;

public sealed record UserRoleAddItem
{
    public required UserId UserId { get; init; }
    public required Role Role { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
