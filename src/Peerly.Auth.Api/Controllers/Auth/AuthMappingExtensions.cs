using System.Diagnostics.CodeAnalysis;
using Peerly.Auth.ApplicationServices.Features.Auth.Login;
using Peerly.Auth.ApplicationServices.Models.Common;
using Proto = Peerly.Auth.V1;

namespace Peerly.Auth.Api.Controllers.Auth;

[ExcludeFromCodeCoverage]
internal static class AuthMappingExtensions
{
    public static LoginCommand ToLoginCommand(this Proto.V1LoginRequest requestProto)
    {
        return new LoginCommand
        {
            Email = requestProto.Email,
            Password = requestProto.Password
        };
    }

    public static Proto.V1LoginResponse ToV1LoginResponse(this CommandResponse<LoginCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1LoginResponse { SuccessResponse = ToSuccessResponse(success) },
            validationError => new Proto.V1LoginResponse
            {
                ValidationError = validationError.ToProto<LoginCommand, Proto.V1LoginRequest>()
            },
            otherError => new Proto.V1LoginResponse {OtherError = otherError.ToProto()});

        static Proto.V1LoginResponse.Types.Success ToSuccessResponse(LoginCommandResponse commandSuccess)
        {
            return new Proto.V1LoginResponse.Types.Success
            {
                Token = new Proto.Token
                {
                    AccessToken = commandSuccess.AuthToken.AccessToken,
                    RefreshToken = commandSuccess.AuthToken.RefreshToken
                },
                UserId = (long)commandSuccess.UserId
            };
        }
    }
}
