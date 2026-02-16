using System;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;
using Peerly.Auth.Persistence.Repositories.Users.Models;

namespace Peerly.Auth.Persistence.Repositories.Users;

internal static class UserRepositoryMapper
{
    public static User? ToUser(this UserDb? userDb)
    {
        if (userDb is null)
        {
            return null;
        }

        return new User
        {
            Id = new UserId(userDb.Id),
            Role = Enum.Parse<Role>(userDb.Role),
            PasswordHash = userDb.PasswordHash
        };
    }

    public static UserIdRole? ToUserIdRole(this UserIdRoleDb? userIdRoleDb)
    {
        if (userIdRoleDb is null)
        {
            return null;
        }

        return new UserIdRole
        {
            Id = new UserId(userIdRoleDb.Id),
            Role = Enum.Parse<Role>(userIdRoleDb.Role)
        };
    }
}
