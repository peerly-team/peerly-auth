using System;
using Peerly.Auth.Models.Sessions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;

internal static class LogoutHandlerMapper
{
    public static SessionFilter ToSessionFilter(this Session session)
    {
        return new SessionFilter
        {
            Id = session.Id,
            RefreshTokenHash = session.RefreshTokenHash
        };
    }

    public static SessionUpdateItem ToSessionUpdateItem(DateTimeOffset currentTime)
    {
        return new SessionUpdateItem
        {
            CancellationTime = currentTime,
            UpdateTime = currentTime
        };
    }
}
