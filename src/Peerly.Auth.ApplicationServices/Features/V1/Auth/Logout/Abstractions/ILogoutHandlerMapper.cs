using Peerly.Auth.Models.Sessions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout.Abstractions;

internal interface ILogoutHandlerMapper
{
    SessionUpdateItem ToSessionUpdateItem();
}
