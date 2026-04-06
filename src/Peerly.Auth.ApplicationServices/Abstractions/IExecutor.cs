using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Auth.ApplicationServices.Abstractions;

internal interface IExecutor<in TRequestItem>
{
    Task RunAsync(TRequestItem requestItem, CancellationToken cancellationToken);
}
