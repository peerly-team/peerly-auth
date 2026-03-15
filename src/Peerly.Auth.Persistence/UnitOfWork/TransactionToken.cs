using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;

namespace Peerly.Auth.Persistence.UnitOfWork;

internal sealed class TransactionToken : IOperationSet
{
    private readonly DbTransaction _transaction;
    private readonly Action? _cleanupAction;
    private bool _disposed;

    public TransactionToken(DbTransaction transaction, Action? cleanupAction)
    {
        ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));
        _transaction = transaction;
        _cleanupAction = cleanupAction;
    }

    public ValueTask DisposeAsync()
    {
        return DisposeAsyncCore();
    }

    public async Task Complete(CancellationToken cancellationToken)
    {
        CheckNotDisposed();
        await _transaction.CommitAsync(cancellationToken);
        await DisposeAsyncCore();
    }

    public async Task Rollback(CancellationToken cancellationToken)
    {
        CheckNotDisposed();
        await _transaction.RollbackAsync(cancellationToken);
        await DisposeAsyncCore();
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        await _transaction.DisposeAsync();
        _cleanupAction?.Invoke();

        _disposed = true;
    }

    private void CheckNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransactionToken));
    }
}
