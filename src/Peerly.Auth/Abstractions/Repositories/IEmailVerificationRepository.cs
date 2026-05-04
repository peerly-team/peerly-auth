using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.EmailVerifications;

namespace Peerly.Auth.Abstractions.Repositories;

public interface IEmailVerificationRepository : IReadOnlyEmailVerificationRepository
{
    Task<bool> AddAsync(EmailVerificationAddItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmailVerificationJobItem>> TakeAsync(EmailVerificationFilter filter, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(UserId userId, Action<IUpdateBuilder<EmailVerificationUpdateItem>> configureUpdate, CancellationToken cancellationToken);
}

public interface IReadOnlyEmailVerificationRepository
{
    Task<UserId?> GetUserIdByTokenAsync(string token, CancellationToken cancellationToken);
    Task<UserExpirationTime?> GetUserExpirationTimeByTokenAsync(string token, CancellationToken cancellationToken);
}
