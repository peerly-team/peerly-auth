namespace Peerly.Auth.Persistence.UnitOfWork;

internal sealed class ConnectionFactoryOptions
{
    public const string SectionName = "ConnectionFactoryOptions";

    public required string MasterHost { get; init; }
    public required int DefaultPort { get; init; }
    public required string Database { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
}
