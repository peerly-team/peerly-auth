namespace Peerly.Auth.Models.Outbox;

public sealed record OutboxMessageFilter
{
    public required string Topic { get; init; }
    public required int Limit { get; init; }
    public int? MaxFailCount { get; init; }
}
