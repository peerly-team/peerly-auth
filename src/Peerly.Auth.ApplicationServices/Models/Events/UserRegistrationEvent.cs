using System;

namespace Peerly.Auth.ApplicationServices.Models.Events;

public sealed record UserRegistrationEvent
{
    public required long Id { get; init; }
    public required int Role { get; init; }
    public required string Email { get; init; }
    public required string? Name { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}
