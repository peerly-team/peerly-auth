using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Peerly.Auth.ApplicationServices.Generators.AuthTokens;

internal static class JwkGenerator
{
    public static IReadOnlyCollection<JsonWebKey> MassCreate(IReadOnlyCollection<RsaSecurityKey> keys)
    {
        var jwks = new List<JsonWebKey>(keys.Count);

        foreach (var key in keys)
        {
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
            jwk.Use = JwtSettings.UseSig;
            jwk.Alg = JwtSettings.Algorithm;

            jwks.Add(jwk);
        }

        return jwks;
    }
}
