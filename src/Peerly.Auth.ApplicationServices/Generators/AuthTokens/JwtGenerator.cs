using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.ApplicationServices.Generators.AuthTokens.Abstractions;
using Peerly.Auth.ApplicationServices.Providers.AuthTokens.Abstractions;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Generators.AuthTokens;

internal sealed class JwtGenerator : IJwtGenerator
{
    private readonly IClock _clock;
    private readonly ISigningKeyProvider _signingKeyProvider;

    public JwtGenerator(IClock clock, ISigningKeyProvider signingKeyProvider)
    {
        _clock = clock;
        _signingKeyProvider = signingKeyProvider;
    }

    public JwtSecurityToken Create(User user)
    {
        var issuedAt = _clock.GetCurrentTime();
        var jwtId = Guid.NewGuid(); // todo: нужно сохранить в БД
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()), // кому принадлежит (постоянное значение)
            new(JwtRegisteredClaimNames.Jti, jwtId.ToString()), // идентификатор JWT
            new(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // время выдачи токена
        };
        claims.AddRange(user.Roles.Select(role => new Claim(JwtSettings.Roles, role.ToString())));

        var rsaKeys = _signingKeyProvider.GetActiveRsaPrivateKey();
        var creds = new SigningCredentials(rsaKeys, JwtSettings.Algorithm);

        return new JwtSecurityToken(
            issuer: JwtSettings.Issuer,
            audience: JwtSettings.Audience,
            claims: claims,
            notBefore: issuedAt.DateTime,
            expires: issuedAt.AddMinutes(15).DateTime,
            signingCredentials: creds);
    }
}
