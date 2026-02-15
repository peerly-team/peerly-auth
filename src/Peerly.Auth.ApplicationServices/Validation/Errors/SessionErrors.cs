using Peerly.Auth.Models.Shared;

namespace Peerly.Auth.ApplicationServices.Validation.Errors;

internal static class SessionErrors
{
    public static ErrorMessage ActiveSessionForUserNotFound => "Нет активных сессий для пользователя";
    public static ErrorMessage RefreshTokenForUserNotFound(string refreshToken) => $"Токен обновления для пользователя {refreshToken} не найден";
}
