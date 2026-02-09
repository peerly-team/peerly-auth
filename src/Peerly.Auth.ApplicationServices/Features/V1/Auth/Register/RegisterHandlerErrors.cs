using Peerly.Auth.Models.Shared;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;

internal static class RegisterHandlerErrors
{
    public static ErrorMessage EmailAlreadyUsed => "Пользователь с таким адресом электронной почты уже существует";
    public static ErrorMessage IncorrectEmailFormat => "Некорректный формат адреса электронной почты";
    public static ErrorMessage PasswordIsTooSimple => "Пароль слишком простой";
}
