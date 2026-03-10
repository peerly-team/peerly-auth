using System.IdentityModel.Tokens.Jwt;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Generators.AuthTokens.Abstractions;

internal interface IJwtGenerator
{
    JwtSecurityToken Create(UserIdRole user);
}
