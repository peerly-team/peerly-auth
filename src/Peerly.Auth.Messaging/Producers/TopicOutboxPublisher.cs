using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.Messaging.Options;
using Peerly.Auth.Models.Outbox;

namespace Peerly.Auth.Messaging.Producers;

// TODO: При масштабировании на несколько экземпляров — добавить pg_advisory_lock (session-level)
//       для leader election, чтобы outbox обрабатывался только одним подом и сохранялся порядок сообщений.
internal abstract class TopicOutboxPublisher : BackgroundService
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly KafkaProducerOptions _options;
    private readonly ILogger _logger;
    private readonly IProducer<string, string> _producer;

    protected abstract string Topic { get; }

    protected TopicOutboxPublisher(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        IOptions<KafkaProducerOptions> options,
        ILoggerFactory loggerFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _options = options.Value;
        _logger = loggerFactory.CreateLogger(GetType());

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        _logger.LogInformation(
            "{Publisher} | Started outbox publisher for topic {Topic}, polling every {Interval}s",
            GetType().Name,
            Topic,
            _options.OutboxPollIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{Publisher} | Error during outbox processing: {ErrorMessage}",
                    GetType().Name,
                    ex.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.OutboxPollIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessOutboxBatchAsync(CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var filter = new OutboxMessageFilter
        {
            Topic = Topic,
            Limit = _options.OutboxBatchSize,
            MaxFailCount = _options.OutboxMaxFailCount
        };
        var messages = await unitOfWork.OutboxRepository.TakeAsync(filter, cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "{Publisher} | Processing {Count} outbox messages",
            GetType().Name,
            messages.Count);

        foreach (var outboxMessage in messages)
        {
            try
            {
                await PublishAndMarkAsync(unitOfWork, outboxMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{Publisher} | Failed to publish message {MessageId} to topic {Topic}: {ErrorMessage}",
                    GetType().Name,
                    outboxMessage.Id,
                    Topic,
                    ex.Message);

                await unitOfWork.OutboxRepository.UpdateAsync(
                    outboxMessage.Id,
                    builder => builder
                        .Set(item => item.IncrementFailCount, true)
                        .Set(item => item.Error, ex.Message),
                    cancellationToken);
            }
        }
    }

    private async Task PublishAndMarkAsync(
        ICommonUnitOfWork unitOfWork,
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = outboxMessage.Key,
            Value = outboxMessage.Payload
        };

        var result = await _producer.ProduceAsync(Topic, kafkaMessage, cancellationToken);

        await unitOfWork.OutboxRepository.UpdateAsync(
            outboxMessage.Id,
            builder => builder.Set(item => item.ProcessedTime, DateTimeOffset.UtcNow),
            cancellationToken);

        _logger.LogInformation(
            "{Publisher} | Published message {MessageId} ({EventType}) to topic {Topic}, partition {Partition}, offset {Offset}",
            GetType().Name,
            outboxMessage.Id,
            outboxMessage.EventType,
            result.Topic,
            result.Partition.Value,
            result.Offset.Value);
    }

    public override void Dispose()
    {
        _producer.Dispose();
        base.Dispose();
    }
}
