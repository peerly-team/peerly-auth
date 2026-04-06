using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Outbox;
using Peerly.Auth.Persistence.Common;
using Peerly.Auth.Persistence.Repositories.Outbox.Models;
using Peerly.Auth.Persistence.UnitOfWork;
using Peerly.Auth.Tools;
using static Peerly.Auth.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Auth.Persistence.Repositories.Outbox;

internal sealed class OutboxRepository : IOutboxRepository
{
    private readonly IConnectionContext _connectionContext;

    public OutboxRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<bool> AddAsync(OutboxMessageAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            item.EventType,
            item.Topic,
            item.Key,
            item.Payload,
            item.CreationTime,
            FailCount = 0
        };

        const string Query =
            $"""
             insert into {OutboxMessageTable.TableName} (
                         {OutboxMessageTable.EventType},
                         {OutboxMessageTable.Topic},
                         {OutboxMessageTable.Key},
                         {OutboxMessageTable.Payload},
                         {OutboxMessageTable.CreationTime},
                         {OutboxMessageTable.FailCount})
                  values (
                         @{nameof(queryParams.EventType)},
                         @{nameof(queryParams.Topic)},
                         @{nameof(queryParams.Key)},
                         @{nameof(queryParams.Payload)},
                         @{nameof(queryParams.CreationTime)},
                         @{nameof(queryParams.FailCount)});
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRow = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRow == 1;
    }

    public async Task<IReadOnlyList<OutboxMessage>> TakeAsync(OutboxMessageFilter filter, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            filter.Topic,
            filter.Limit,
            filter.MaxFailCount
        };

        const string Query =
            $"""
             select {OutboxMessageTable.Id},
                    {OutboxMessageTable.EventType},
                    {OutboxMessageTable.Key},
                    {OutboxMessageTable.Payload}
               from {OutboxMessageTable.TableName}
              where {OutboxMessageTable.ProcessedTime} is null
                and {OutboxMessageTable.Topic} = @{nameof(queryParams.Topic)}
                and (@{nameof(queryParams.MaxFailCount)} is null
                    or {OutboxMessageTable.FailCount} < @{nameof(queryParams.MaxFailCount)})
              order by {OutboxMessageTable.Id}
              limit @{nameof(queryParams.Limit)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var dbs = await _connectionContext.Connection.QueryAsync<OutboxMessageDb>(command);

        return dbs.ToArrayBy(db => db.ToOutboxMessage());
    }

    public async Task<bool> UpdateAsync(
        OutboxMessageId id,
        Action<IUpdateBuilder<OutboxMessageUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<OutboxMessageUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(id)}", (long)id);

        var query =
            $"""
             update {OutboxMessageTable.TableName} as new
                set {OutboxMessageTable.ProcessedTime} = case
                    when {configuration.GetFlagParamName(item => item.ProcessedTime)}
                    then {configuration.GetParamName(item => item.ProcessedTime)}
                    else {OutboxMessageTable.ProcessedTime} end,
                    {OutboxMessageTable.FailCount} = case
                    when {configuration.GetFlagParamName(item => item.IncrementFailCount)}
                    then {OutboxMessageTable.FailCount} + 1
                    else {OutboxMessageTable.FailCount} end,
                    {OutboxMessageTable.Error} = case
                    when {configuration.GetFlagParamName(item => item.Error)}
                    then {configuration.GetParamName(item => item.Error)}
                    else {OutboxMessageTable.Error} end
               from (select {OutboxMessageTable.Id}
                       from {OutboxMessageTable.TableName}
                      where {OutboxMessageTable.Id} = @{nameof(id)}
                        for update) as old
              where new.{OutboxMessageTable.Id} = old.{OutboxMessageTable.Id};
             """;

        var command = new CommandDefinition(
            query,
            queryParams,
            _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var affectedRows = await _connectionContext.Connection.ExecuteAsync(command);

        return affectedRows == 1;
    }
}
