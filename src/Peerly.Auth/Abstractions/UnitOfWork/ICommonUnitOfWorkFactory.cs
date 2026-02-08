using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Auth.Abstractions.UnitOfWork;

public interface ICommonUnitOfWorkFactory
{
    Task<ICommonUnitOfWork> Create(CancellationToken cancellationToken);
}
