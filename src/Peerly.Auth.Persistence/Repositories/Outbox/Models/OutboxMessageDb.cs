namespace Peerly.Auth.Persistence.Repositories.Outbox.Models;

internal sealed record OutboxMessageDb
{
    public required long Id { get; init; }
    public required string EventType { get; init; }
    public required string Key { get; init; }
    public required string Payload { get; init; }
}
