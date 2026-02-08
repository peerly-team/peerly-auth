using Peerly.Auth.Abstractions.Repositories;

namespace Peerly.Auth.Abstractions.UnitOfWork;

public interface ICommonUnitOfWork : IUnitOfWork
{
    IUserRepository UserRepository { get; }
}

public interface ICommonReadOnlyUnitOfWork : IUnitOfWork
{
    IReadOnlyUserRepository ReadOnlyUserRepository { get; }
}
