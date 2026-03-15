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
            Role = item.Role.ToString(),
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {UserTable.TableName} (
                         {UserTable.Email},
                         {UserTable.PasswordHash},
                         {UserTable.Name},
                         {UserTable.Role},
                         {UserTable.CreationTime})
                  values (
                         @{nameof(queryParams.Email)},
                         @{nameof(queryParams.PasswordHash)},
                         @{nameof(queryParams.Name)},
                         @{nameof(queryParams.Role)},
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

    public async Task<UserIdRole?> GetRoleAsync(UserId userId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            UserId = (long)userId
        };

        const string Query =
            $"""
             select {UserTable.Id},
                    {UserTable.Role}
               from {UserTable.TableName}
              where {UserTable.Id} = @{nameof(queryParams.UserId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var userIdRoleDb = await _connectionContext.Connection.QuerySingleOrDefaultAsync<UserIdRoleDb>(command);

        return userIdRoleDb.ToUserIdRole();
    }

    public async Task<User?> GetAsync(string email, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            Email = email
        };

        const string Query =
            $"""
             select {UserTable.Id},
                    {UserTable.PasswordHash},
                    {UserTable.Role}
               from {UserTable.TableName}
              where {UserTable.Email} = @{nameof(queryParams.Email)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var userDb = await _connectionContext.Connection.QuerySingleOrDefaultAsync<UserDb>(command);

        return userDb.ToUser();
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
