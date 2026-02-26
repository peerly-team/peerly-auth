using OneOf.Types;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;

public sealed record LogoutCommand : ICommand<Success>
{
    public required UserId UserId { get; init; }
    public required string RefreshToken { get; init; }
}
