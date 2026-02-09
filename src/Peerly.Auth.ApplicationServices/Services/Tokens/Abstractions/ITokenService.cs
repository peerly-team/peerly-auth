using Peerly.Auth.Models.Auth;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;

internal interface ITokenService
{
    AuthToken CreateAuthToken(User user);
    string CreateEmailVerificationToken();
}
