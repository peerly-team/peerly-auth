using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;
using Peerly.Auth.Persistence.Common;
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
            Role = item.UserRole.ToString(),
            item.IsConfirmed,
            item.CreationTime
        };

        const string Query =
            $"""
             insert into {UserTable.TableName} (
                         {UserTable.Email},
                         {UserTable.PasswordHash},
                         {UserTable.Role},
                         {UserTable.IsConfirmed},
                         {UserTable.CreationTime})
                  values (
                         @{nameof(queryParams.Email)},
                         @{nameof(queryParams.PasswordHash)},
                         @{nameof(queryParams.Role)},
                         @{nameof(queryParams.IsConfirmed)},
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

    public async Task<UserRole?> GetUserRoleAsync(UserId userId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            UserId = (long)userId
        };

        const string Query =
            $"""
             select {UserTable.Role}
               from {UserTable.TableName}
              where {UserTable.Id} = @{nameof(queryParams.UserId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var db = await _connectionContext.Connection.QuerySingleOrDefaultAsync<UserRoleDb>(command);

        return db?.ToUserRole();
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            Email = email
        };

        const string Query =
            $"""
             select {UserTable.Id},
                    {UserTable.PasswordHash},
                    {UserTable.Role},
                    {UserTable.IsConfirmed}
               from {UserTable.TableName}
              where {UserTable.Email} = @{nameof(queryParams.Email)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);
        var db = await _connectionContext.Connection.QuerySingleOrDefaultAsync<UserDb>(command);

        return db?.ToUser();
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

    public async Task<bool> IsEmailConfirmedAsync(UserId userId, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            UserId = (long)userId
        };

        const string Query =
            $"""
             select {UserTable.IsConfirmed}
               from {UserTable.TableName}
              where {UserTable.Id} = @{nameof(queryParams.UserId)};
             """;

        var command = new CommandDefinition(
            commandText: Query,
            parameters: queryParams,
            transaction: _connectionContext.Transaction,
            cancellationToken: cancellationToken);

        return await _connectionContext.Connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> UpdateAsync(
        UserId userId,
        Action<IUpdateBuilder<UserUpdateItem>> configureUpdate,
        CancellationToken cancellationToken)
    {
        var builder = new UpdateBuilder<UserUpdateItem>();
        configureUpdate(builder);

        var configuration = builder.Build();
        var queryParams = configuration.GetQueryParams();
        queryParams.Add($"@{nameof(userId)}", (long)userId);

        var query =
            $"""
             update {UserTable.TableName} as new
                set {UserTable.UpdateTime} = now(),
                    {UserTable.IsConfirmed} = case
                    when {configuration.GetFlagParamName(item => item.IsConfirmed)}
                    then {configuration.GetParamName(item => item.IsConfirmed)}
                    else {UserTable.IsConfirmed} end
               from (select {UserTable.Id}
                       from {UserTable.TableName}
                      where {UserTable.Id} = @{nameof(userId)}
                        for update) as old
              where new.{UserTable.Id} = old.{UserTable.Id};
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
