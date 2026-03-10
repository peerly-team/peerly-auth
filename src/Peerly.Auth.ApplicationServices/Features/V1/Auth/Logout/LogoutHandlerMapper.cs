using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout.Abstractions;
using Peerly.Auth.Models.Sessions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;

internal sealed class LogoutHandlerMapper : ILogoutHandlerMapper
{
    private readonly IClock _clock;

    public LogoutHandlerMapper(IClock clock)
    {
        _clock = clock;
    }

    public static SessionFilter ToSessionFilter(Session session)
    {
        return new SessionFilter
        {
            Id = session.Id,
            RefreshTokenHash = session.RefreshTokenHash
        };
    }

    public SessionUpdateItem ToSessionUpdateItem()
    {
        var currentTime = _clock.GetCurrentTime();

        return new SessionUpdateItem
        {
            CancellationTime = currentTime,
            UpdateTime = currentTime
        };
    }
}
