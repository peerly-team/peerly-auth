using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Auth.ApplicationServices.Services.Abstractions;

internal interface IHashService
{
    Task<string> HashAsync(string text, CancellationToken cancellationToken);
    bool Verify(string text, string hashedText);
}
