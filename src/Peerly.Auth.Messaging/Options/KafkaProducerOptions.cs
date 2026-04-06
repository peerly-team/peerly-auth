namespace Peerly.Auth.Messaging.Options;

internal sealed class KafkaProducerOptions
{
    public const string SectionName = "KafkaProducer";

    public required string BootstrapServers { get; init; }
    public int OutboxPollIntervalSeconds { get; init; } = 5;
    public int OutboxBatchSize { get; init; } = 100;
    public int OutboxMaxFailCount { get; init; } = 5;
}
