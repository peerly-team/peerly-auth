using System;

namespace Peerly.Auth.Abstractions.ApplicationServices;

public interface IClock
{
    DateTimeOffset GetCurrentMoscowDateTime();
    DateTimeOffset GetCurrentTime();
}
