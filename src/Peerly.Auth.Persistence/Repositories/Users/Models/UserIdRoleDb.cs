namespace Peerly.Auth.Persistence.Repositories.Users.Models;

internal sealed record UserIdRoleDb
{
    public required long Id { get; init; }
    public required string Role { get; init; }
}
