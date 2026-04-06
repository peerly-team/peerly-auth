using System;
using Peerly.Auth.Models.Email;

namespace Peerly.Auth.Persistence.Repositories.EmailVerifications.Models;

internal sealed record EmailVerificationJobItemDb
{
    public required long Id { get; init; }
    public required string Token { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }

    public EmailVerificationJobItem ToJobItem()
    {
        return new EmailVerificationJobItem
        {
            Id = Id,
            Token = Token,
            Email = Email,
            Name = Name,
            ExpirationTime = ExpirationTime
        };
    }
}
