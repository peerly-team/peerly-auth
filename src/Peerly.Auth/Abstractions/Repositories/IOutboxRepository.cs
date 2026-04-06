using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Outbox;

namespace Peerly.Auth.Abstractions.Repositories;

public interface IOutboxRepository
{
    Task<bool> AddAsync(OutboxMessageAddItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<OutboxMessage>> TakeAsync(OutboxMessageFilter filter, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(OutboxMessageId id, Action<IUpdateBuilder<OutboxMessageUpdateItem>> configureUpdate, CancellationToken cancellationToken);
}
