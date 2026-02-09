using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.Abstractions.Repositories;

public interface IUserRoleRepository : IReadOnlyUserRoleRepository
{
    Task<bool> BatchAddAsync(IReadOnlyCollection<UserRoleAddItem> items, CancellationToken cancellationToken);
}

public interface IReadOnlyUserRoleRepository
{

}
