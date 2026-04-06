using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Email;
using Peerly.Auth.Models.Outbox;
using Peerly.Auth.Models.Sessions;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Register.Abstractions;

internal interface IRegisterHandlerMapper
{
    UserAddItem ToUserAddItem(RegisterCommand command, string passwordHash);
    EmailVerificationAddItem ToEmailVerificationAddItem(UserId userId, string emailVerificationTokenHash);
    SessionAddItem ToSessionAddItem(UserId userId, string refreshTokenHash);
    OutboxMessageAddItem ToOutboxMessage(UserId userId, RegisterCommand command);
}
