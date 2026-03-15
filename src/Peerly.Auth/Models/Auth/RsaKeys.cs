using Microsoft.IdentityModel.Tokens;

namespace Peerly.Auth.Models.Auth;

public sealed record RsaKeys
{
    public required RsaSecurityKey PrivateKey { get; init; }
    public required RsaSecurityKey PublicKey { get; init; }
}
