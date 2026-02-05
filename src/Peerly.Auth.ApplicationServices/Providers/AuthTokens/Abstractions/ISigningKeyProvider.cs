using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Peerly.Auth.ApplicationServices.Providers.AuthTokens.Abstractions;

internal interface ISigningKeyProvider
{
    RsaSecurityKey GetActiveRsaPrivateKey();
    IReadOnlyCollection<RsaSecurityKey> GetRsaPublicKeys();
}
