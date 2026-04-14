using System;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;
using Peerly.Auth.Persistence.Repositories.Users.Models;

namespace Peerly.Auth.Persistence.Repositories.Users;

internal static class UserRepositoryMapper
{
    public static User ToUser(this UserDb db)
    {
        return new User
        {
            Id = new UserId(db.Id),
            Role = Enum.Parse<UserRole>(db.Role),
            PasswordHash = db.PasswordHash,
            IsConfirmed = db.IsConfirmed
        };
    }

    public static UserRole? ToUserRole(this UserRoleDb db)
    {
        return Enum.Parse<UserRole>(db.Role);
    }
}
