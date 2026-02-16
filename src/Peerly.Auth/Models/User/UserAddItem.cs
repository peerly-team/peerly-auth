using System;

namespace Peerly.Auth.Models.User;

public sealed record UserAddItem
{
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public required string Name { get; init; }
    public required Role Role { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
