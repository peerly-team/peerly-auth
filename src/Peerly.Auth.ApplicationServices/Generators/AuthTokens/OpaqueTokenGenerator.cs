using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Peerly.Auth.ApplicationServices.Generators.AuthTokens;

internal static class OpaqueTokenGenerator
{
    public static string Run(int sizeBytes = 32)
    {
        var data = RandomNumberGenerator.GetBytes(sizeBytes);
        return WebEncoders.Base64UrlEncode(data);
    }
}
