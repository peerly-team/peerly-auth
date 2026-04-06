using System;
using System.Collections.Generic;
using Peerly.Auth.Models.BackgroundService;

namespace Peerly.Auth.Models.Email;

public sealed record EmailVerificationFilter
{
    public required IReadOnlyCollection<ProcessStatus> ProcessStatuses { get; init; }
    public required int? MaxFailCount { get; init; }
    public required TimeSpan? ProcessTimeoutSeconds { get; init; }
    public required int? Limit { get; init; }

    public static EmailVerificationFilter Empty()
    {
        return new EmailVerificationFilter
        {
            ProcessStatuses = [],
            MaxFailCount = null,
            ProcessTimeoutSeconds = null,
            Limit = null
        };
    }
}
