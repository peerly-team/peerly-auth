using Peerly.Auth.Abstractions.Repositories;

namespace Peerly.Auth.Abstractions.UnitOfWork;

public interface ICommonUnitOfWork : IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IEmailVerificationRepository EmailVerificationRepository { get; }
    ISessionRepository SessionRepository { get; }
    IOutboxRepository OutboxRepository { get; }
}

public interface ICommonReadOnlyUnitOfWork : IUnitOfWork
{
    IReadOnlyUserRepository ReadOnlyUserRepository { get; }
    IReadOnlyEmailVerificationRepository ReadOnlyEmailVerificationRepository { get; }
    IReadOnlySessionRepository ReadOnlySessionRepository { get; }
}
