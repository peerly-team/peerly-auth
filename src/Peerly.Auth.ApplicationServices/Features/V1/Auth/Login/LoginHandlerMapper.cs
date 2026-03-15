using Microsoft.Extensions.Options;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login.Abstractions;
using Peerly.Auth.ApplicationServices.Options;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Sessions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;

internal sealed class LoginHandlerMapper : ILoginHandlerMapper
{
    private readonly IClock _clock;
    private readonly ExpirationTimeOptions _options;

    public LoginHandlerMapper(IClock clock, IOptions<ExpirationTimeOptions> options)
    {
        _clock = clock;
        _options = options.Value;
    }

    public static SessionFilter ToSessionFilter(Session session)
    {
        return SessionFilter.Empty() with
        {
            Id = session.Id
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

    public SessionAddItem ToSessionAddItem(UserId userId, string refreshTokenHash)
    {
        var currentTime = _clock.GetCurrentTime();

        return new SessionAddItem
        {
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            ExpirationTime = currentTime.AddDays(_options.RefreshTokenPeriodDays),
            CreationTime = currentTime
        };
    }
}
