using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Auth.Abstractions.UnitOfWork;

public interface ICommonUnitOfWorkFactory
{
    Task<ICommonUnitOfWork> CreateAsync(CancellationToken cancellationToken);
    Task<ICommonReadOnlyUnitOfWork> CreateReadOnlyAsync(CancellationToken cancellationToken);
}
