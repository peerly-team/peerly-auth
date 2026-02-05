using System.Collections.Generic;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.User;

public sealed record User
{
    public required UserId UserId { get; init; }
    public required IReadOnlyCollection<Role> Roles { get; init; }
}
