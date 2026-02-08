using System;
using System.Data.Common;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;

namespace Peerly.Auth.Persistence.UnitOfWork;

internal sealed class CommonUnitOfWork : UnitOfWork, ICommonUnitOfWork, ICommonReadOnlyUnitOfWork
{
    private readonly Lazy<IUserRepository> _userRepository;

    public CommonUnitOfWork(
        DbConnection connection,
        Func<IConnectionContext, IUserRepository> userRepositoryFactory) : base(connection)
    {
        _userRepository = new Lazy<IUserRepository>(() => userRepositoryFactory(this));
    }

    public IUserRepository UserRepository => _userRepository.Value;

    public IReadOnlyUserRepository ReadOnlyUserRepository => _userRepository.Value;
}
