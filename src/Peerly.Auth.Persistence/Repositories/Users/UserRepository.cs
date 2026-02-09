using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;
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

    public async Task<UserId?> GetAsync(string email, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            Email = email
        };

        const string Query =
            $"""
             select id
               from users
              where email = @{nameof(queryParams.Email)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var userId = await _connectionContext.Connection.QuerySingleOrDefaultAsync<long?>(command);

        return userId is null ? null : new UserId(userId.Value);
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
