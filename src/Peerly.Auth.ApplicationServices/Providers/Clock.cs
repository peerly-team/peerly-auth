using System;
using Peerly.Auth.Abstractions.ApplicationServices;

namespace Peerly.Auth.ApplicationServices.Providers;

internal sealed class Clock : IClock
{
    public DateTimeOffset GetCurrentMoscowDateTime()
    {
        var russianStandardTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            DateTime.Now,
            TimeZoneInfo.Local.Id,
            "Russian Standard Time");
        return new DateTimeOffset(russianStandardTime);
    }

    public DateTimeOffset GetCurrentTime()
    {
        return DateTimeOffset.UtcNow;
    }
}
