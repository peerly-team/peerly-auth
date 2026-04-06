using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Outbox;
using Peerly.Auth.Persistence.Repositories.Outbox.Models;

namespace Peerly.Auth.Persistence.Repositories.Outbox;

internal static class OutboxRepositoryMapper
{
    public static OutboxMessage ToOutboxMessage(this OutboxMessageDb db)
    {
        return new OutboxMessage
        {
            Id = new OutboxMessageId(db.Id),
            EventType = db.EventType,
            Key = db.Key,
            Payload = db.Payload
        };
    }
}
