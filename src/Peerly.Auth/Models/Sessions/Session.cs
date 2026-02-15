namespace Peerly.Auth.Models.Sessions;

public sealed record Session
{
    public required long Id { get; init; }
    public required string RefreshTokenHash { get; init; }
}
