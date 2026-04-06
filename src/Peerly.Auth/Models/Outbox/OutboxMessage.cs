using Peerly.Auth.Identifiers;

namespace Peerly.Auth.Models.Outbox;

public sealed record OutboxMessage
{
    public required OutboxMessageId Id { get; init; }
    public required string EventType { get; init; }
    public required string Key { get; init; }
    public required string Payload { get; init; }
}
