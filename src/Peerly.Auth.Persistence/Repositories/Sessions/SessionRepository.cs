using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Sessions;
using Peerly.Auth.Persistence.UnitOfWork;
using static Peerly.Auth.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Auth.Persistence.Repositories.Sessions;

internal sealed class SessionRepository : ISessionRepository
{
    private readonly IConnectionContext _connectionContext;

    public SessionRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<bool> AddAsync(SessionAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            UserId = (long)item.UserId,
            item.RefreshTokenHash,
            item.CreationTime,
            item.ExpirationTime
        };

        const string Query =
            $"""
             insert into {SessionTable.TableName} (
                         {SessionTable.UserId},
                         {SessionTable.RefreshTokenHash},
                         {SessionTable.CreationTime},
                         {SessionTable.ExpirationTime})
                  values (
                         @{nameof(queryParams.UserId)},
                         @{nameof(queryParams.RefreshTokenHash)},
                         @{nameof(queryParams.CreationTime)},
                         @{nameof(queryParams.ExpirationTime)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRow = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRow == 1;
    }

    public async Task<bool> UpdateAsync(SessionFilter filter, SessionUpdateItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            filter.Id,
            filter.RefreshTokenHash,
            item.UpdateTime,
            item.CancellationTime
        };

        const string Query =
            $"""
             update {SessionTable.TableName}
                set {SessionTable.CancellationTime} = @{nameof(queryParams.CancellationTime)},
                    {SessionTable.UpdateTime} = @{nameof(queryParams.UpdateTime)}
              where (@{nameof(queryParams.Id)} is null
                    or {SessionTable.Id} = @{nameof(queryParams.Id)})
                and (@{nameof(queryParams.RefreshTokenHash)} is null
                    or {SessionTable.RefreshTokenHash} = @{nameof(queryParams.RefreshTokenHash)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRow = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRow == 1;
    }

    public async Task<Session?> GetAsync(UserId userId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            UserId = (long)userId
        };

        const string Query =
            $"""
             select {SessionTable.Id},
                    {SessionTable.RefreshTokenHash}
               from {SessionTable.TableName}
              where {SessionTable.UserId} = @{nameof(queryParams.UserId)}
                and {SessionTable.CancellationTime} is null;
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.QuerySingleOrDefaultAsync<Session>(command);
    }
}
