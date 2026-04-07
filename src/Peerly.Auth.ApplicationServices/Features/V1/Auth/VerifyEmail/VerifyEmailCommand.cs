using OneOf.Types;
using Peerly.Auth.ApplicationServices.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.VerifyEmail;

public sealed record VerifyEmailCommand : ICommand<Success>
{
    public required string Token { get; init; }
}
