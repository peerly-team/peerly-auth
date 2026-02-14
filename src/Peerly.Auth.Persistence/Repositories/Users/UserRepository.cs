using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;
using Peerly.Auth.Persistence.Repositories.Users.Models;
using Peerly.Auth.Persistence.UnitOfWork;
using static Peerly.Auth.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Auth.Persistence.Repositories.Users;

internal sealed class UserRepository : IUserRepository
{
    private readonly IConnectionContext _connectionContext;

    public UserRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<UserId> AddAsync(UserAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            item.Email,
            item.PasswordHash,
            item.Name,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {UserTable.TableName} (
                         {UserTable.Email},
                         {UserTable.PasswordHash},
                         {UserTable.Name},
                         {UserTable.CreationTime})
                  values (
                         @{nameof(queryParams.Email)},
                         @{nameof(queryParams.PasswordHash)},
                         @{nameof(queryParams.Name)},
                         @{nameof(queryParams.CreationTime)})
               returning {UserTable.Id};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var id = await _connectionContext.Connection.QuerySingleAsync<long>(command);

        return new UserId(id);
    }

    public async Task<User?> GetAsync(string email, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            Email = email
        };

        const string Query =
            $"""
             select u.{UserTable.Id},
                    u.{UserTable.PasswordHash},
                    ur.{UserRoleTable.RoleId}
               from {UserTable.TableName} u
               join {UserRoleTable.TableName} ur on ur.{UserRoleTable.UserId} = u.{UserTable.Id}
              where u.{UserTable.Email} = @{nameof(queryParams.Email)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var usersDb = await _connectionContext.Connection.QueryAsync<UserDb>(command);

        return usersDb.ToUser();
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            Email = email
        };

        const string Query =
            $"""
             select exists(select
                             from {UserTable.TableName}
                            where {UserTable.Email} = @{nameof(queryParams.Email)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.ExecuteScalarAsync<bool>(command);
    }

}
