using Peerly.Auth.Models.Shared;

namespace Peerly.Auth.ApplicationServices.Validation.Errors;

internal static class EmailErrors
{
    public static ErrorMessage NotFound => "Неверный адрес электронной почты или пароль";
    public static ErrorMessage EmailAlreadyUsed => "Пользователь с таким адресом электронной почты уже существует";
    public static ErrorMessage IncorrectEmailFormat => "Некорректный формат адреса электронной почты";
}
