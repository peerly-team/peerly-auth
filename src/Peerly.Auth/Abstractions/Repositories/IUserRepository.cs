using System;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.Abstractions.Repositories;

public interface IUserRepository : IReadOnlyUserRepository
{
    Task<UserId> AddAsync(UserAddItem item, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(UserId userId, Action<IUpdateBuilder<UserUpdateItem>> configureUpdate, CancellationToken cancellationToken);
}

public interface IReadOnlyUserRepository
{
    Task<UserRole?> GetUserRoleAsync(UserId userId, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken);
    Task<bool> IsEmailConfirmedAsync(UserId userId, CancellationToken cancellationToken);
}
