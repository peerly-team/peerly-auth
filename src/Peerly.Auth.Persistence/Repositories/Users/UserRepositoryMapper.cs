using System.Collections.Generic;
using System.Linq;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;
using Peerly.Auth.Persistence.Repositories.Users.Models;
using Peerly.Auth.Tools;

namespace Peerly.Auth.Persistence.Repositories.Users;

internal static class UserRepositoryMapper
{
    public static User? ToUser(this IEnumerable<UserDb> userDbs)
    {
        var userGroup = userDbs.GroupBy(userDb => (userDb.id, userDb.password_hash)).SingleOrDefault();
        if (userGroup is null)
        {
            return null;
        }

        return new User
        {
            Id = (UserId)userGroup.Key.id,
            Roles = userGroup.ToArrayBy(userDb => (Role)userDb.role_id),
            PasswordHash = userGroup.Key.password_hash
        };
    }
}
