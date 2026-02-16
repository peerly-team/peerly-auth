using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register.Abstractions;
using Peerly.Auth.ApplicationServices.Options;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Email;
using Peerly.Auth.Models.Sessions;
using Peerly.Auth.Models.User;
using Peerly.Auth.Tools;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;

internal sealed class RegisterHandlerMapper : IRegisterHandlerMapper
{
    private readonly IClock _clock;
    private readonly ExpirationTimeOptions _options;

    public RegisterHandlerMapper(IClock clock, IOptions<ExpirationTimeOptions> options)
    {
        _clock = clock;
        _options = options.Value;
    }

    public static UserIdRole ToUserIdRole(UserId userId, IReadOnlyCollection<Role> roles)
    {
        return new UserIdRole
        {
            Id = userId,
            Roles = roles
        };
    }

    public UserAddItem ToUserAddItem(RegisterCommand command, string passwordHash)
    {
        return new UserAddItem
        {
            Email = command.Email,
            PasswordHash = passwordHash,
            Name = command.UserName,
            CreationTime = _clock.GetCurrentTime()
        };
    }

    public IReadOnlyCollection<UserRoleAddItem> ToUserRoleAddItems(UserId userId, IReadOnlyCollection<Role> roles)
    {
        return roles.ToArrayBy(
            role => new UserRoleAddItem
            {
                UserId = userId,
                Role = role,
                CreationTime = _clock.GetCurrentTime()
            });
    }

    public EmailVerificationAddItem ToEmailVerificationAddItem(UserId userId, string emailVerificationTokenHash)
    {
        var currentTime = _clock.GetCurrentTime();

        return new EmailVerificationAddItem
        {
            UserId = userId,
            TokenHash = emailVerificationTokenHash,
            CreationTime = currentTime,
            ExpirationTime = currentTime.AddMinutes(_options.EmailVerificationTokenValidityPeriodMinutes)
        };
    }

    public SessionAddItem ToSessionAddItem(UserId userId, string refreshTokenHash)
    {
        var currentTime = _clock.GetCurrentTime();

        return new SessionAddItem
        {
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            ExpirationTime = currentTime,
            CreationTime = currentTime.AddDays(_options.RefreshTokenPeriodDays)
        };
    }
}
