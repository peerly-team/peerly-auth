using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Auth.ApplicationServices.Abstractions;

internal interface IMassExecutor<in TRequestItem>
{
    Task RunAsync(IReadOnlyCollection<TRequestItem> requestItems, CancellationToken cancellationToken);
}
