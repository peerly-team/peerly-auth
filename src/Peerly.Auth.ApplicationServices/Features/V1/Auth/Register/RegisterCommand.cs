using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;

public sealed record RegisterCommand : ICommand<RegisterCommandResponse>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string UserName { get; init; }
    public required Role Role { get; init; }
}
