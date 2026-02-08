using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Peerly.Auth.Models.UnitOfWork;

namespace Peerly.Auth.Persistence.UnitOfWork;

internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowUnacceptedIsolationLevel(TransactionRequirements requirements, IsolationLevel currentIsolationLevel)
    {
        throw new InvalidOperationException(
            $"Could not obtain a transaction with '{requirements.AcceptableIsolationLevel}' isolation level as there is an " +
            $"already running transaction with '{currentIsolationLevel}' isolation level which is not satisfied by the " +
            $"provided '{requirements.IsolationLevelPolicy}' policy. " +
            "Consider providing another isolation level and/or policy values.");
    }
}
