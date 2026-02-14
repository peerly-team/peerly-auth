namespace Peerly.Auth.Models.User;

public sealed record User : UserIdRole
{
    public required string PasswordHash { get; init; }
}
