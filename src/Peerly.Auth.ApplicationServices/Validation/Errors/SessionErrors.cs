using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Shared;

namespace Peerly.Auth.ApplicationServices.Validation.Errors;

internal static class SessionErrors
{
    public static ErrorMessage ActiveSessionForUserNotFound(UserId userId) => $"Нет активных сессий для пользователя {userId}";

    public static ErrorMessage RefreshTokenForUserNotFound(string refreshToken, UserId userId) =>
        $"""Токен обновления "{refreshToken}" для пользователя {userId} не найден""";
}
