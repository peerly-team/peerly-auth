using Microsoft.IdentityModel.Tokens;

namespace Peerly.Auth.ApplicationServices.Generators.AuthTokens;

internal static class JwtSettings
{
    public const string Issuer = "peerly-auth";
    public const string Audience = "peerly-gateway";
    public const string Algorithm = SecurityAlgorithms.RsaSha256;
    public const string Roles = "roles";
    public const string UseSig = "sig";
}
