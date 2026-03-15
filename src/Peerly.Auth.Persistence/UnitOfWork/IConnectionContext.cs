using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.Models.UnitOfWork;

namespace Peerly.Auth.Persistence.UnitOfWork;

internal interface IConnectionContext
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }
    Task<IOperationSet> StartOperationSet(CancellationToken cancellationToken);
    Task<IOperationSet> StartOperationSet(TransactionRequirements requirements, CancellationToken cancellationToken);
}
