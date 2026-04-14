namespace Peerly.Auth.Persistence.Repositories.Users.Models;

internal sealed record UserRoleDb
{
    public required string Role { get; init; }
}
