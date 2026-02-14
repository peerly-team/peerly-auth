namespace Peerly.Auth.Persistence.Repositories.Users.Models;

internal sealed record UserDb
{
    public required long id { get; init; }
    public required string password_hash { get; init; }
    public required long role_id { get; init; }
}
