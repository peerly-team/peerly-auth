namespace Peerly.Auth.Persistence.Repositories.Users.Models;

internal sealed record UserDb
{
    public required long Id { get; init; }
    public required string PasswordHash { get; init; }
    public required string Role { get; init; }
}
