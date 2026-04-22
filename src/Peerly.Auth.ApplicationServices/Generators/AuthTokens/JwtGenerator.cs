using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.ApplicationServices.Generators.AuthTokens.Abstractions;
using Peerly.Auth.ApplicationServices.Options;
using Peerly.Auth.ApplicationServices.Providers.AuthTokens.Abstractions;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Generators.AuthTokens;

internal sealed class JwtGenerator : IJwtGenerator
{
    private readonly IClock _clock;
    private readonly ISigningKeyProvider _signingKeyProvider;
    private readonly ExpirationTimeOptions _options;

    public JwtGenerator(IClock clock, ISigningKeyProvider signingKeyProvider, IOptions<ExpirationTimeOptions> options)
    {
        _clock = clock;
        _signingKeyProvider = signingKeyProvider;
        _options = options.Value;
    }

    public JwtSecurityToken Create(UserId userId, UserRole userRole)
    {
        var issuedAt = _clock.GetCurrentTime();
        var jwtId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()), // кому принадлежит (постоянное значение)
            new(JwtRegisteredClaimNames.Jti, jwtId.ToString()), // идентификатор JWT
            new(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // время выдачи токена
            new("role", userRole.ToString())
        };

        var rsaKeys = _signingKeyProvider.GetActiveRsaPrivateKey();
        var creds = new SigningCredentials(rsaKeys, JwtSettings.Algorithm);

        return new JwtSecurityToken(
            issuer: JwtSettings.Issuer,
            audience: JwtSettings.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: issuedAt.AddMinutes(_options.AccessTokenPeriodMinutes).UtcDateTime,
            signingCredentials: creds);
    }
}
