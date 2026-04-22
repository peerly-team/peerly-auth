using System;

namespace Peerly.Auth.Models.User;

public sealed record UserAddItem
{
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public required UserRole UserRole { get; init; }
    public required bool IsConfirmed { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
