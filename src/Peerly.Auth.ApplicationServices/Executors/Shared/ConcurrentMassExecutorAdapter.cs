using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Executors.Shared.Abstractions;

namespace Peerly.Auth.ApplicationServices.Executors.Shared;

internal sealed class ConcurrentMassExecutorAdapter<TRequest, TOptions> : IMassExecutor<TRequest>
    where TOptions : class, IMassExecutorOptions
{
    private readonly IExecutor<TRequest> _executor;
    private readonly TOptions _options;

    public ConcurrentMassExecutorAdapter(IExecutor<TRequest> executor, IOptionsSnapshot<TOptions> optionsSnapshot)
    {
        _executor = executor;
        _options = optionsSnapshot.Value;
    }

    public async Task RunAsync(IReadOnlyCollection<TRequest> requestItems, CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(
            requestItems,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism
            },
            async (request, cancelToken) =>
            {
                await _executor.RunAsync(request, cancelToken);
            });
    }
}
