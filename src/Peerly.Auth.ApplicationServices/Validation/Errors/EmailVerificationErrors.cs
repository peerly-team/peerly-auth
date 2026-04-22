using Peerly.Auth.Models.Shared;

namespace Peerly.Auth.ApplicationServices.Validation.Errors;

internal static class EmailVerificationErrors
{
    public static ErrorMessage TokenExpired => "Срок действия ссылки для подтверждения истёк";
}
