using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;

public sealed record LogoutQuery : IQuery<LogoutQueryResponse>
{
    public required UserId UserId { get; init; }
    public required string RefreshToken { get; init; }
}
