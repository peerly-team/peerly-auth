using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Models.Email;
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

    public async Task<EmailVerificationInfo?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var queryParams = new { Token = token };

        const string Query =
            $"""
             select {EmailVerificationTable.Id},
                    {EmailVerificationTable.ExpirationTime},
                    {EmailVerificationTable.VerificationTime}
               from {EmailVerificationTable.TableName}
              where {EmailVerificationTable.Token} = @{nameof(queryParams.Token)}
              limit 1;
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var db = await _connectionContext.Connection.QueryFirstOrDefaultAsync<EmailVerificationInfoDb>(command);

        return db?.ToModel();
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
             with cte as (select {EmailVerificationTable.Id}
                            from {EmailVerificationTable.TableName}
                           where (cardinality(@{nameof(queryParams.ProcessStatuses)}) = 0
                                 or {EmailVerificationTable.ProcessStatus} = any(@{nameof(queryParams.ProcessStatuses)}))
                             and (@{nameof(queryParams.MaxFailCount)} is null
                                 or {EmailVerificationTable.FailCount} < @{nameof(queryParams.MaxFailCount)})
                             and (@{nameof(queryParams.ProcessTimeoutSeconds)} is null
                                 or {EmailVerificationTable.TakenTime} < now() - (@{nameof(queryParams.ProcessTimeoutSeconds)} || ' seconds')::interval
                                 or {EmailVerificationTable.TakenTime} is null)
                           order by {EmailVerificationTable.Id}
                             for update skip locked
                           limit @{nameof(queryParams.Limit)}),
             updated as (update {EmailVerificationTable.TableName} as ev
                            set {EmailVerificationTable.ProcessStatus} = 'InProgress',
                                {EmailVerificationTable.TakenTime} = now()
                           from cte
                          where ev.{EmailVerificationTable.Id} = cte.{EmailVerificationTable.Id}
                      returning ev.{EmailVerificationTable.Id},
                                ev.{EmailVerificationTable.UserId},
                                ev.{EmailVerificationTable.Token},
                                ev.{EmailVerificationTable.ExpirationTime})
             select updated.{EmailVerificationTable.Id},
                    updated.{EmailVerificationTable.Token},
                    updated.{EmailVerificationTable.ExpirationTime},
                    u.{UserTable.Email},
                    u.{UserTable.Name}
               from updated
               join {UserTable.TableName} u on updated.{EmailVerificationTable.UserId} = u.{UserTable.Id};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var dbs = await _connectionContext.Connection.QueryAsync<EmailVerificationJobItemDb>(command);

        return dbs.ToArrayBy(db => db.ToJobItem());
    }

    public async Task<bool> UpdateAsync(
        long id,
        Action<IUpdateBuilder<EmailVerificationUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<EmailVerificationUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(id)}", id);

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
                    else {EmailVerificationTable.Error} end,
                    {EmailVerificationTable.VerificationTime} = case
                    when {configuration.GetFlagParamName(item => item.VerificationTime)}
                    then {configuration.GetParamName(item => item.VerificationTime)}::timestamptz
                    else {EmailVerificationTable.VerificationTime} end
               from (select {EmailVerificationTable.Id}
                       from {EmailVerificationTable.TableName}
                      where {EmailVerificationTable.Id} = @{nameof(id)}
                        for update) as old
              where new.{EmailVerificationTable.Id} = old.{EmailVerificationTable.Id};
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
