using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Models.Email;
using Peerly.Auth.Persistence.UnitOfWork;
using static Peerly.Auth.Persistence.Schemas.PeerlyCommonScheme;

namespace Peerly.Auth.Persistence.Repositories.EmailVerifications;

internal sealed class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly IConnectionContext _connectionContext;

    public EmailVerificationRepository(IConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public async Task<bool> AddAsync(EmailVerificationAddItem item, CancellationToken cancellationToken)
    {
        var queryParams = new
        {
            UserId = (long)item.UserId,
            item.TokenHash,
            item.CreationTime,
            item.ExpirationTime
        };

        const string Query =
            $"""
             insert into {EmailVerificationTable.TableName} (
                         {EmailVerificationTable.UserId},
                         {EmailVerificationTable.TokenHash},
                         {EmailVerificationTable.CreationTime},
                         {EmailVerificationTable.ExpirationTime})
                  values (
                         @{nameof(queryParams.UserId)},
                         @{nameof(queryParams.TokenHash)},
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
}
