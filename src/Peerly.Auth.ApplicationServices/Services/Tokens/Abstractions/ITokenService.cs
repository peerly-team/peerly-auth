using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Auth;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;

internal interface ITokenService
{
    AuthToken CreateAuthToken(UserId userId, UserRole userRole);
    string CreateAccessToken(UserId userId, UserRole userRole);
    string CreateRefreshToken();
}
