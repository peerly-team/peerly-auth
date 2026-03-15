namespace Peerly.Auth.Models.Auth;

public sealed record AuthToken
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
