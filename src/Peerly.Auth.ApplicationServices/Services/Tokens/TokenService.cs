using System.IdentityModel.Tokens.Jwt;
using Peerly.Auth.ApplicationServices.Generators.AuthTokens;
using Peerly.Auth.ApplicationServices.Generators.AuthTokens.Abstractions;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;
using Peerly.Auth.Models.Auth;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Services.Tokens;

internal sealed class TokenService : ITokenService
{
    private readonly IJwtGenerator _jwtGenerator;

    public TokenService(IJwtGenerator jwtGenerator)
    {
        _jwtGenerator = jwtGenerator;
    }

    public AuthToken CreateAuthToken(User user)
    {
        var jwt = _jwtGenerator.Create(user);

        return new AuthToken
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
            RefreshToken = OpaqueTokenGenerator.Run()
        };
    }

    public string CreateEmailVerificationToken()
    {
        return OpaqueTokenGenerator.Run();
    }
}
