using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.EmailVerifications;
using Peerly.Auth.Persistence.Repositories.EmailVerifications.Models;

namespace Peerly.Auth.Persistence.Repositories.EmailVerifications;

internal static class EmailVerificationRepositoryMapper
{
    public static UserExpirationTime ToUserExpirationTime(this UserExpirationTimeDb db)
    {
        return new UserExpirationTime
        {
            UserId = new UserId(db.UserId),
            ExpirationTime = db.ExpirationTime
        };
    }

    public static EmailVerificationJobItem ToEmailVerificationJobItem(this EmailVerificationJobItemDb db)
    {
        return new EmailVerificationJobItem
        {
            UserId = new UserId(db.UserId),
            Token = db.Token,
            Email = db.Email,
            ExpirationTime = db.ExpirationTime
        };
    }
}
