using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Sessions;

namespace Peerly.Auth.Abstractions.Repositories;

public interface ISessionRepository : IReadOnlySessionRepository
{
    Task<bool> AddAsync(SessionAddItem item, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(SessionFilter filter, SessionUpdateItem item, CancellationToken cancellationToken);
}

public interface IReadOnlySessionRepository
{
    Task<Session?> GetAsync(UserId userId, CancellationToken cancellationToken);
}
