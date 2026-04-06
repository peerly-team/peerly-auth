using System;
using System.Data.Common;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;

namespace Peerly.Auth.Persistence.UnitOfWork;

internal sealed class CommonUnitOfWork : UnitOfWork, ICommonUnitOfWork, ICommonReadOnlyUnitOfWork
{
    private readonly Lazy<IUserRepository> _userRepository;
    private readonly Lazy<IEmailVerificationRepository> _emailVerificationRepository;
    private readonly Lazy<ISessionRepository> _sessionRepository;
    private readonly Lazy<IOutboxRepository> _outboxRepository;

    public CommonUnitOfWork(
        DbConnection connection,
        Func<IConnectionContext, IUserRepository> userRepositoryFactory,
        Func<IConnectionContext, IEmailVerificationRepository> emailVerificationRepositoryFactory,
        Func<IConnectionContext, ISessionRepository> sessionRepositoryFactory,
        Func<IConnectionContext, IOutboxRepository> outboxRepositoryFactory) : base(connection)
    {
        _userRepository = new Lazy<IUserRepository>(() => userRepositoryFactory(this));
        _emailVerificationRepository = new Lazy<IEmailVerificationRepository>(() => emailVerificationRepositoryFactory(this));
        _sessionRepository = new Lazy<ISessionRepository>(() => sessionRepositoryFactory(this));
        _outboxRepository = new Lazy<IOutboxRepository>(() => outboxRepositoryFactory(this));
    }

    public IUserRepository UserRepository => _userRepository.Value;
    public IEmailVerificationRepository EmailVerificationRepository => _emailVerificationRepository.Value;
    public ISessionRepository SessionRepository => _sessionRepository.Value;
    public IOutboxRepository OutboxRepository => _outboxRepository.Value;

    public IReadOnlyUserRepository ReadOnlyUserRepository => _userRepository.Value;
    public IReadOnlyEmailVerificationRepository ReadOnlyEmailVerificationRepository => _emailVerificationRepository.Value;
    public IReadOnlySessionRepository ReadOnlySessionRepository => _sessionRepository.Value;
}
