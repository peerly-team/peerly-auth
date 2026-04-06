using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.Constants;
using Peerly.Auth.Messaging.Options;

namespace Peerly.Auth.Messaging.Producers.UserRegistrationEvents;

internal sealed class UserRegistrationEventsPublisher : TopicOutboxPublisher
{
    protected override string Topic => OutboxTopics.UserRegistrationEvents;

    public UserRegistrationEventsPublisher(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        IOptions<KafkaProducerOptions> options,
        ILoggerFactory loggerFactory)
        : base(unitOfWorkFactory, options, loggerFactory)
    {
    }
}
