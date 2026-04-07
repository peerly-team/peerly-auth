using System;
using Peerly.Auth.Models.Email;

namespace Peerly.Auth.Persistence.Repositories.EmailVerifications.Models;

internal sealed record EmailVerificationInfoDb
{
    public required long Id { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
    public DateTimeOffset? VerificationTime { get; init; }

    public EmailVerificationInfo ToModel()
    {
        return new EmailVerificationInfo
        {
            Id = Id,
            ExpirationTime = ExpirationTime,
            VerificationTime = VerificationTime
        };
    }
}
