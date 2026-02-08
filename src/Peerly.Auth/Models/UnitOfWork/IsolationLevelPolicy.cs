namespace Peerly.Auth.Models.UnitOfWork;

public enum IsolationLevelPolicy
{
    RequireExact = 1,
    OneOf = 2,
    AllowAny = 3
}
