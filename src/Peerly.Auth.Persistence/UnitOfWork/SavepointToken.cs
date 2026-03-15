using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;

namespace Peerly.Auth.Persistence.UnitOfWork;

internal sealed class SavepointToken : IOperationSet
{
    private readonly DbTransaction _transaction;
    private string? _savepointName;

    private SavepointToken(DbTransaction transaction, string savepointName)
    {
        _transaction = transaction;
        _savepointName = savepointName;
    }

    public static async Task<SavepointToken> Create(DbTransaction transaction, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

        var savepointName = Guid.NewGuid().ToString();

        await transaction.SaveAsync(savepointName, cancellationToken);

        return new SavepointToken(transaction, savepointName);
    }

    public async ValueTask DisposeAsync()
    {
        if (_savepointName is null)
            return;

        await _transaction.RollbackAsync(_savepointName!);
        _savepointName = null;
    }

    public async Task Complete(CancellationToken cancellationToken)
    {
        if (_savepointName is null)
        {
            throw new InvalidOperationException(
                "Could not complete the operation set as it is already completed, rollbacked and/or disposed.");
        }

        await _transaction.ReleaseAsync(_savepointName!, cancellationToken);
        _savepointName = null;
    }

    public async Task Rollback(CancellationToken cancellationToken)
    {
        if (_savepointName is null)
        {
            throw new InvalidOperationException(
                "Could not rollback the operation set as it is already completed, rollbacked and/or disposed.");
        }

        await _transaction.RollbackAsync(_savepointName!, cancellationToken);
        _savepointName = null;
    }
}
