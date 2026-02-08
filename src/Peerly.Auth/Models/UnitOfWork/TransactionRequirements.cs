using System;
using System.Data;

namespace Peerly.Auth.Models.UnitOfWork;

public sealed class TransactionRequirements
{
    public static TransactionRequirements RepeatableReadLevel { get; } = new()
    {
        DesiredIsolationLevel = IsolationLevel.RepeatableRead,
        AcceptableIsolationLevel = IsolationLevel.RepeatableRead
    };

    public IsolationLevel DesiredIsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
    public IsolationLevel AcceptableIsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
    public IsolationLevelPolicy IsolationLevelPolicy { get; set; } = IsolationLevelPolicy.RequireExact;

    public bool CheckIsolationLevelAccepted(IsolationLevel currentIsolationLevel)
    {
        return IsolationLevelPolicy switch
        {
            IsolationLevelPolicy.RequireExact => currentIsolationLevel == AcceptableIsolationLevel,
            IsolationLevelPolicy.OneOf => (int)currentIsolationLevel == ((int)currentIsolationLevel & (int)AcceptableIsolationLevel),
            IsolationLevelPolicy.AllowAny => true,
            _ => throw new InvalidOperationException(
                $"'{IsolationLevelPolicy}' isolation level policy is not supported. Consider adding another branch or using other values.")
        };
    }
}
