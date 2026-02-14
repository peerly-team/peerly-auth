using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Peerly.Auth.Abstractions.UnitOfWork;

namespace Peerly.Auth.Persistence.UnitOfWork;

internal sealed class CommonUnitOfWorkFactory : ICommonUnitOfWorkFactory
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly Func<DbConnection, CommonUnitOfWork> _unitOfWorkInnerFactory;

    public CommonUnitOfWorkFactory(
        Func<DbConnection, CommonUnitOfWork> unitOfWorkInnerFactory,
        NpgsqlDataSource dataSource)
    {
        _unitOfWorkInnerFactory = unitOfWorkInnerFactory;
        _dataSource = dataSource;
    }

    public async Task<ICommonUnitOfWork> CreateAsync(CancellationToken cancellationToken)
    {
        return await CreateUnitOfWork(cancellationToken);
    }

    public async Task<ICommonReadOnlyUnitOfWork> CreateReadOnlyAsync(CancellationToken cancellationToken)
    {
        return await CreateUnitOfWork(cancellationToken);
    }

    private async Task<CommonUnitOfWork> CreateUnitOfWork(CancellationToken cancellationToken)
    {
        var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        try
        {
            return _unitOfWorkInnerFactory(connection);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
