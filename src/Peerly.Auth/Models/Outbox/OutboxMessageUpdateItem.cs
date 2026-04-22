using System;

namespace Peerly.Auth.Models.Outbox;

public sealed record OutboxMessageUpdateItem
{
    public required DateTimeOffset ProcessedTime { get; init; }
    public required bool IncrementFailCount { get; init; }
    public required string Error { get; init; }
}
