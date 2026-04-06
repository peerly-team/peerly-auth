using System.Text.Json;
using Microsoft.Extensions.Options;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Events;
using Peerly.Auth.ApplicationServices.Options;
using Peerly.Auth.Constants;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Email;
using Peerly.Auth.Models.Outbox;
using Peerly.Auth.Models.Sessions;
using Peerly.Auth.Models.User;

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

    public static UserIdRole ToUserIdRole(UserId userId, Role role)
    {
        return new UserIdRole
        {
            Id = userId,
            Role = role
        };
    }

    public UserAddItem ToUserAddItem(RegisterCommand command, string passwordHash)
    {
        return new UserAddItem
        {
            Email = command.Email,
            PasswordHash = passwordHash,
            Name = command.UserName,
            Role = command.Role,
            CreationTime = _clock.GetCurrentTime()
        };
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

    public OutboxMessageAddItem ToOutboxMessage(UserId userId, RegisterCommand command)
    {
        var registrationEvent = new UserRegistrationEvent
        {
            Id = (long)userId,
            Role = (int)command.Role,
            Email = command.Email,
            Name = command.UserName,
            Timestamp = _clock.GetCurrentTime()
        };

        return new OutboxMessageAddItem
        {
            EventType = nameof(UserRegistrationEvent),
            Topic = KafkaTopics.UserRegistrationEvents,
            Key = userId.ToString(),
            Payload = JsonSerializer.Serialize(registrationEvent),
            CreationTime = _clock.GetCurrentTime()
        };
    }
}
