using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Models.Sessions;

namespace Peerly.Auth.Abstractions.Repositories;

public interface ISessionRepository : IReadOnlySessionRepository
{
    Task<bool> AddAsync(SessionAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlySessionRepository
{

}
