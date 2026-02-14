using System.Collections.Generic;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.User;

public record UserIdRole
{
    public required UserId Id { get; init; }
    public required IReadOnlyCollection<Role> Roles { get; init; }
}
