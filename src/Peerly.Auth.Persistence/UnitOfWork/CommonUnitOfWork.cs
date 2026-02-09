using System;
using System.Data.Common;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;

namespace Peerly.Auth.Persistence.UnitOfWork;

internal sealed class CommonUnitOfWork : UnitOfWork, ICommonUnitOfWork, ICommonReadOnlyUnitOfWork
{
    private readonly Lazy<IUserRepository> _userRepository;
    private readonly Lazy<IUserRoleRepository> _userRoleRepository;
    private readonly Lazy<IEmailVerificationRepository> _emailVerificationRepository;
    private readonly Lazy<ISessionRepository> _sessionRepository;

    public CommonUnitOfWork(
        DbConnection connection,
        Func<IConnectionContext, IUserRepository> userRepositoryFactory,
        Func<IConnectionContext, IUserRoleRepository> userRoleRepositoryFactory,
        Func<IConnectionContext, IEmailVerificationRepository> emailVerificationRepositoryFactory,
        Func<IConnectionContext, ISessionRepository> sessionRepositoryFactory) : base(connection)
    {
        _userRepository = new Lazy<IUserRepository>(() => userRepositoryFactory(this));
        _userRoleRepository = new Lazy<IUserRoleRepository>(() => userRoleRepositoryFactory(this));
        _emailVerificationRepository = new Lazy<IEmailVerificationRepository>(() => emailVerificationRepositoryFactory(this));
        _sessionRepository = new Lazy<ISessionRepository>(() => sessionRepositoryFactory(this));
    }

    public IUserRepository UserRepository => _userRepository.Value;
    public IUserRoleRepository UserRoleRepository => _userRoleRepository.Value;
    public IEmailVerificationRepository EmailVerificationRepository => _emailVerificationRepository.Value;
    public ISessionRepository SessionRepository => _sessionRepository.Value;

    public IReadOnlyUserRepository ReadOnlyUserRepository => _userRepository.Value;
    public IReadOnlyUserRoleRepository ReadOnlyUserRoleRepository => _userRoleRepository.Value;
    public IReadOnlyEmailVerificationRepository ReadOnlyEmailVerificationRepository => _emailVerificationRepository.Value;
    public IReadOnlySessionRepository ReadOnlySessionRepository => _sessionRepository.Value;
}
