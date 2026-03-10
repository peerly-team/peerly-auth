using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.User;

public record UserIdRole
{
    public required UserId Id { get; init; }
    public required Role Role { get; init; }
}
