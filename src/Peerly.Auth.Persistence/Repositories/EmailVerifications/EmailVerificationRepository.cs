using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.EmailVerifications;
using Peerly.Auth.Persistence.Common;
using Peerly.Auth.Persistence.Repositories.EmailVerifications.Models;
using Peerly.Auth.Persistence.UnitOfWork;
using Peerly.Auth.Tools;
using static Peerly.Auth.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Auth.Persistence.Repositories.EmailVerifications;

internal sealed class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly IConnectionContext _connectionContext;

    public EmailVerificationRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<UserExpirationTime?> GetUserExpirationTimeByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var queryParams = new { Token = token };

        const string Query =
            $"""
             select {EmailVerificationTable.UserId},
                    {EmailVerificationTable.ExpirationTime}
               from {EmailVerificationTable.TableName}
              where {EmailVerificationTable.Token} = @{nameof(queryParams.Token)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var db = await _connectionContext.Connection.QuerySingleOrDefaultAsync<UserExpirationTimeDb>(command);

        return db?.ToUserExpirationTime();
    }

    public async Task<bool> AddAsync(EmailVerificationAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            UserId = (long)item.UserId,
            item.Token,
            ProcessStatus = item.ProcessStatus.ToString(),
            item.FailCount,
            item.CreationTime,
            item.ExpirationTime
        };

        const string Query =
            $"""
             insert into {EmailVerificationTable.TableName} (
                         {EmailVerificationTable.UserId},
                         {EmailVerificationTable.Token},
                         {EmailVerificationTable.ProcessStatus},
                         {EmailVerificationTable.FailCount},
                         {EmailVerificationTable.CreationTime},
                         {EmailVerificationTable.ExpirationTime})
                  values (
                         @{nameof(queryParams.UserId)},
                         @{nameof(queryParams.Token)},
                         @{nameof(queryParams.ProcessStatus)},
                         @{nameof(queryParams.FailCount)},
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

    public async Task<IReadOnlyList<EmailVerificationJobItem>> TakeAsync(
        EmailVerificationFilter filter,
        CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            ProcessStatuses = filter.ProcessStatuses.ToArrayBy(processStatus => processStatus.ToString()),
            filter.MaxFailCount,
            filter.ProcessTimeoutSeconds,
            filter.Limit
        };

        const string Query =
            $"""
             with cte as (select {EmailVerificationTable.UserId}
                            from {EmailVerificationTable.TableName}
                           where (cardinality(@{nameof(queryParams.ProcessStatuses)}) = 0
                                 or {EmailVerificationTable.ProcessStatus} = any(@{nameof(queryParams.ProcessStatuses)}))
                             and (@{nameof(queryParams.MaxFailCount)} is null
                                 or {EmailVerificationTable.FailCount} < @{nameof(queryParams.MaxFailCount)})
                             and (@{nameof(queryParams.ProcessTimeoutSeconds)} is null
                                 or {EmailVerificationTable.TakenTime} < now() - (@{nameof(queryParams.ProcessTimeoutSeconds)} || ' seconds')::interval
                                 or {EmailVerificationTable.TakenTime} is null)
                           order by {EmailVerificationTable.UserId}
                             for update skip locked
                           limit @{nameof(queryParams.Limit)})
             update {EmailVerificationTable.TableName} as ev
                set {EmailVerificationTable.ProcessStatus} = 'InProgress',
                    {EmailVerificationTable.TakenTime} = now()
               from cte
               join {UserTable.TableName} u on cte.{EmailVerificationTable.UserId} = u.{UserTable.Id}
              where ev.{EmailVerificationTable.UserId} = cte.{EmailVerificationTable.UserId}
             returning ev.{EmailVerificationTable.UserId},
                       ev.{EmailVerificationTable.Token},
                       ev.{EmailVerificationTable.ExpirationTime},
                       u.{UserTable.Email}
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var dbs = await _connectionContext.Connection.QueryAsync<EmailVerificationJobItemDb>(command);

        return dbs.ToArrayBy(db => db.ToEmailVerificationJobItem());
    }

    public async Task<bool> UpdateAsync(
        UserId userId,
        Action<IUpdateBuilder<EmailVerificationUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<EmailVerificationUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(userId)}", (long)userId);

        var query =
            $"""
             update {EmailVerificationTable.TableName} as new
                set {EmailVerificationTable.ProcessStatus} = case
                    when {configuration.GetFlagParamName(item => item.ProcessStatus)}
                    then {configuration.GetParamName(item => item.ProcessStatus)}
                    else {EmailVerificationTable.ProcessStatus} end,
                    {EmailVerificationTable.FailCount} = case
                    when {configuration.GetFlagParamName(item => item.IncrementFailCount)}
                    then {EmailVerificationTable.FailCount} + 1
                    else {EmailVerificationTable.FailCount} end,
                    {EmailVerificationTable.Error} = case
                    when {configuration.GetFlagParamName(item => item.Error)}
                    then {configuration.GetParamName(item => item.Error)}
                    else {EmailVerificationTable.Error} end
               from (select {EmailVerificationTable.UserId}
                       from {EmailVerificationTable.TableName}
                      where {EmailVerificationTable.UserId} = @{nameof(userId)}
                        for update) as old
              where new.{EmailVerificationTable.UserId} = old.{EmailVerificationTable.UserId};
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
