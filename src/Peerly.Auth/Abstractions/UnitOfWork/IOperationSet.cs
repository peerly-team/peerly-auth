using System;
using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Auth.Abstractions.UnitOfWork;

public interface IOperationSet : IAsyncDisposable
{
    Task Complete(CancellationToken cancellationToken);
    Task Rollback(CancellationToken cancellationToken);
}
