using System;

namespace Peerly.Auth.Persistence.Repositories.EmailVerifications.Models;

internal sealed record UserExpirationTimeDb
{
    public required long UserId { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
}
