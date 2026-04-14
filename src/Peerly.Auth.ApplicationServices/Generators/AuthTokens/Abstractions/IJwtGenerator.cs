using System.IdentityModel.Tokens.Jwt;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Generators.AuthTokens.Abstractions;

internal interface IJwtGenerator
{
    JwtSecurityToken Create(UserId userId, UserRole userRole);
}
