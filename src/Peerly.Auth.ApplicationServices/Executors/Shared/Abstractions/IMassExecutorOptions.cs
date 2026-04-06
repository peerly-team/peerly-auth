namespace Peerly.Auth.ApplicationServices.Executors.Shared.Abstractions;

internal interface IMassExecutorOptions
{
    int MaxDegreeOfParallelism { get; }
}
