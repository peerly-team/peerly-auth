using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Models.User;
using Peerly.Auth.Persistence.UnitOfWork;
using Peerly.Auth.Tools;
using static Peerly.Auth.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Auth.Persistence.Repositories.UserRoles;

internal sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly IConnectionContext _connectionContext;

    public UserRoleRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<bool> BatchAddAsync(IReadOnlyCollection<UserRoleAddItem> items, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            UserIds = items.ToArrayBy(item => (long)item.UserId),
            RoleIds = items.ToArrayBy(item => (int)item.Role),
            CreationTimes = items.ToArrayBy(item => item.CreationTime)
        };

        const string Query =
            $"""
              insert into {UserRoleTable.TableName} (
                          {UserRoleTable.UserId},
                          {UserRoleTable.RoleId},
                          {UserRoleTable.CreationTime})
                   select *
                     from unnest(
                          @{nameof(queryParams.UserIds)},
                          @{nameof(queryParams.RoleIds)},
                          @{nameof(queryParams.CreationTimes)});
              """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == items.Count;
    }
}
