using Peerly.Auth.Abstractions.Repositories;

namespace Peerly.Auth.Abstractions.UnitOfWork;

public interface ICommonUnitOfWork : IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IUserRoleRepository UserRoleRepository { get; }
    IEmailVerificationRepository EmailVerificationRepository { get; }
    ISessionRepository SessionRepository { get; }
}

public interface ICommonReadOnlyUnitOfWork : IUnitOfWork
{
    IReadOnlyUserRepository ReadOnlyUserRepository { get; }
    IReadOnlyUserRoleRepository ReadOnlyUserRoleRepository { get; }
    IReadOnlyEmailVerificationRepository ReadOnlyEmailVerificationRepository { get; }
    IReadOnlySessionRepository ReadOnlySessionRepository { get; }
}
