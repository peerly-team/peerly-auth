using System;

namespace Peerly.Auth.Models.Outbox;

public sealed record OutboxMessageAddItem
{
    public required string EventType { get; init; }
    public required string Topic { get; init; }
    public required string Key { get; init; }
    public required string Payload { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
