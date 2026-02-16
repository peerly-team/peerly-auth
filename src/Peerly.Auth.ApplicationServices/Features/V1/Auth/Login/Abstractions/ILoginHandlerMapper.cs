using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Sessions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Login.Abstractions;

internal interface ILoginHandlerMapper
{
    SessionUpdateItem ToSessionUpdateItem();
    SessionAddItem ToSessionAddItem(UserId userId, string refreshTokenHash);
}
