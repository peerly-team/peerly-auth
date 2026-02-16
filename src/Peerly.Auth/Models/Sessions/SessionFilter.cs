namespace Peerly.Auth.Models.Sessions;

public sealed record SessionFilter
{
    public required long? Id { get; init; }
    public required string? RefreshTokenHash { get; init; }

    public static SessionFilter Empty()
    {
        return new SessionFilter
        {
            Id = null,
            RefreshTokenHash = null
        };
    }
}
