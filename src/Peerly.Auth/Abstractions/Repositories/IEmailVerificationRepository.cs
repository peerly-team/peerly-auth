using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Models.Email;

namespace Peerly.Auth.Abstractions.Repositories;

public interface IEmailVerificationRepository : IReadOnlyEmailVerificationRepository
{
    Task<bool> AddAsync(EmailVerificationAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyEmailVerificationRepository
{

}
