using System.ComponentModel.DataAnnotations;

namespace Peerly.Auth.ApplicationServices.Validation.Validators;

internal static class EmailValidator
{
    public static bool IsValid(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && new EmailAddressAttribute().IsValid(email);
    }
}
