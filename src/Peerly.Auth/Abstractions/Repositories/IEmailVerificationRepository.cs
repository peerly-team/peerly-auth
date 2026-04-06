using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Models.Email;

namespace Peerly.Auth.Abstractions.Repositories;

public interface IEmailVerificationRepository : IReadOnlyEmailVerificationRepository
{
    Task<bool> AddAsync(EmailVerificationAddItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmailVerificationJobItem>> TakeAsync(EmailVerificationFilter filter, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(long id, Action<IUpdateBuilder<EmailVerificationUpdateItem>> configureUpdate, CancellationToken cancellationToken);
}

public interface IReadOnlyEmailVerificationRepository
{

}
