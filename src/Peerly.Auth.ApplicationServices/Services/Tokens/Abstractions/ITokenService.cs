using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Models.Auth;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;

internal interface ITokenService
{
    Task<AuthToken> CreateAuthTokenAsync(User user, CancellationToken cancellationToken);
}
