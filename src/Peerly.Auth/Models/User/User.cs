using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.User;

public sealed record User
{
    public required UserId Id { get; init; }
    public required UserRole Role { get; init; }
    public required string PasswordHash { get; init; }
    public required bool IsConfirmed { get; init; }
}
