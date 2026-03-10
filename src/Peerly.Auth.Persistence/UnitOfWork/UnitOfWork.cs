using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.Models.UnitOfWork;

namespace Peerly.Auth.Persistence.UnitOfWork;

internal abstract class UnitOfWork : IUnitOfWork, IConnectionContext
{
    private readonly DbConnection _connection;
    private DbTransaction? _transaction;
    private bool _disposed;

    protected UnitOfWork(DbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection, nameof(connection));
        _connection = connection;
    }

    public IDbConnection Connection => _connection;
    public IDbTransaction? Transaction => _transaction;

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        await _connection.DisposeAsync();
        _disposed = true;
    }

    public Task<IOperationSet> StartOperationSet(CancellationToken cancellationToken)
    {
        return StartOperationSetCore(new TransactionRequirements(), cancellationToken);
    }

    public Task<IOperationSet> StartOperationSet(TransactionRequirements requirements, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requirements, nameof(requirements));
        return StartOperationSetCore(requirements, cancellationToken);
    }

    // TODO: реализовать корректную обработку ситуаций, когда внешняя операция принудительно завершается до вложенной
    private async Task<IOperationSet> StartOperationSetCore(TransactionRequirements requirements, CancellationToken cancellationToken)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));

        if (_transaction is not null)
        {
            if (!requirements.CheckIsolationLevelAccepted(_transaction.IsolationLevel))
            {
                ThrowHelper.ThrowUnacceptedIsolationLevel(requirements, _transaction.IsolationLevel);
            }

            return await SavepointToken.Create(_transaction, cancellationToken);
        }

        _transaction = await _connection.BeginTransactionAsync(requirements.DesiredIsolationLevel, cancellationToken);

        return new TransactionToken(_transaction, () => _transaction = null);
    }
}
