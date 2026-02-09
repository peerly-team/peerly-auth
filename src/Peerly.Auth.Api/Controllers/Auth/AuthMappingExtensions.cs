using System;
using System.Diagnostics.CodeAnalysis;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.GetJwks;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.Models.Auth;
using Peerly.Auth.Models.User;
using Peerly.Auth.Tools;
using Proto = Peerly.Auth.V1;

namespace Peerly.Auth.Api.Controllers.Auth;

[ExcludeFromCodeCoverage]
internal static class AuthMappingExtensions
{
    public static RegisterCommand ToRegisterCommand(this Proto.V1RegisterRequest requestProto)
    {
        return new RegisterCommand
        {
            Email = requestProto.Email,
            Password = requestProto.Password,
            UserName = requestProto.UserName,
            Roles = requestProto.Roles.ToArrayBy(role => role.ToModel())
        };
    }

    public static Proto.V1RegisterResponse ToV1RegisterResponse(this CommandResponse<RegisterCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1RegisterResponse { SuccessResponse = ToSuccessResponse(success) },
            validationError => new Proto.V1RegisterResponse
            {
                ValidationError = validationError.ToProto<RegisterCommand, Proto.V1RegisterRequest>()
            },
            otherError => new Proto.V1RegisterResponse { OtherError = otherError.ToProto() });

        static Proto.V1RegisterResponse.Types.Success ToSuccessResponse(RegisterCommandResponse commandSuccess)
        {
            return new Proto.V1RegisterResponse.Types.Success
            {
                Token = commandSuccess.AuthToken.ToTokenProto(),
                UserId = (long)commandSuccess.UserId
            };
        }
    }

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
            otherError => new Proto.V1LoginResponse { OtherError = otherError.ToProto() });

        static Proto.V1LoginResponse.Types.Success ToSuccessResponse(LoginCommandResponse commandSuccess)
        {
            return new Proto.V1LoginResponse.Types.Success
            {
                Token = commandSuccess.AuthToken.ToTokenProto(),
                UserId = (long)commandSuccess.UserId
            };
        }
    }

    public static GetJwksQuery ToGetJwksQuery(this Proto.V1GetJwksRequest _)
    {
        return new GetJwksQuery();
    }

    public static Proto.V1GetJwksResponse ToV1GetJwksResponse(this GetJwksQueryResponse queryResponse)
    {
        return new Proto.V1GetJwksResponse
        {
            Jwks = { queryResponse.Jwks }
        };
    }

    private static Proto.Token ToTokenProto(this AuthToken token)
    {
        return new Proto.Token
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken
        };
    }

    private static Role ToModel(this Proto.Role roleProto)
    {
        return roleProto switch
        {
            Proto.Role.Admin => Role.Admin,
            Proto.Role.Teacher => Role.Teacher,
            Proto.Role.Student => Role.Student,
            _ => throw new ArgumentOutOfRangeException(nameof(roleProto), roleProto, null)
        };
    }
}
