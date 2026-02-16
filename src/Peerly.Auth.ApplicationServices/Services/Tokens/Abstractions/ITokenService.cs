using Peerly.Auth.Models.Auth;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;

internal interface ITokenService
{
    AuthToken CreateAuthToken(UserIdRole user);
    string CreateAccessToken(User user);
    string CreateRefreshToken();
}
