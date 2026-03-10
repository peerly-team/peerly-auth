using Peerly.Auth.Models.Shared;

namespace Peerly.Auth.ApplicationServices.Validation.Errors;

internal static class PasswordErrors
{
    public static ErrorMessage Incorrect => "Неверный адрес электронной почты или пароль";
    public static ErrorMessage IsTooSimple => "Пароль слишком простой";
}
