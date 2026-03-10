using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.Abstractions.Repositories;

public interface IUserRepository : IReadOnlyUserRepository
{
    Task<UserId> AddAsync(UserAddItem item, CancellationToken cancellationToken);
}

public interface IReadOnlyUserRepository
{
    Task<UserIdRole?> GetRoleAsync(UserId userId, CancellationToken cancellationToken);
    Task<User?> GetAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken);
}
