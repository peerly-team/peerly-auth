using OneOf.Types;
using Peerly.Auth.ApplicationServices.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.ConfirmEmail;

public sealed record ConfirmEmailCommand : ICommand<Success>
{
    public required string Token { get; init; }
}
