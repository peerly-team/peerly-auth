using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Peerly.Auth.ApplicationServices.Providers.AuthTokens.Abstractions;
using Peerly.Auth.ApplicationServices.Providers.AuthTokens.Options;
using Peerly.Auth.Exceptions;

namespace Peerly.Auth.ApplicationServices.Providers.AuthTokens;

internal sealed class SigningKeyProvider : ISigningKeyProvider
{
    private readonly SigningOptions _signingOptions;

    public SigningKeyProvider(IOptions<SigningOptions> options)
    {
        _signingOptions = options.Value;
    }

    public RsaSecurityKey GetActiveRsaPrivateKey()
    {
        var signingKey = _signingOptions.SigningKeys.FirstOrDefault(signingKey => signingKey.IsActive);
        if (signingKey is null)
        {
            throw new NotFoundException("Не удалось найти активного ключа шифрования!");
        }

        var rsa = RSA.Create();
        rsa.ImportFromPem(signingKey.PrivateKey);
        return new RsaSecurityKey(rsa) { KeyId = signingKey.Kid };
    }

    public IReadOnlyCollection<RsaSecurityKey> GetRsaPublicKeys()
    {
        var keys = new List<RsaSecurityKey>();

        foreach (var signingKey in _signingOptions.SigningKeys)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(signingKey.PrivateKey);

            var publicKey = new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = signingKey.Kid };
            keys.Add(publicKey);
        }

        return keys;
    }
}
