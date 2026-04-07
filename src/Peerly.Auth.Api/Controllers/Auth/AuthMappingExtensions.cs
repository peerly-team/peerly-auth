using System;
using System.Diagnostics.CodeAnalysis;
using OneOf.Types;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.GetJwks;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.RefreshAccessToken;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.VerifyEmail;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Auth;
using Peerly.Auth.Models.User;
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
            Role = requestProto.Role.ToModel()
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

    public static LogoutCommand ToLogoutQuery(this Proto.V1LogoutRequest requestProto)
    {
        return new LogoutCommand
        {
            UserId = new UserId(requestProto.UserId),
            RefreshToken = requestProto.RefreshToken
        };
    }

    public static Proto.V1LogoutResponse ToV1LogoutResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1LogoutResponse { SuccessResponse = new Proto.V1LogoutResponse.Types.Success() },
            validationError => new Proto.V1LogoutResponse
            {
                ValidationError = validationError.ToProto<LogoutCommand, Proto.V1LogoutRequest>()
            },
            otherError => new Proto.V1LogoutResponse { OtherError = otherError.ToProto() });
    }

    public static RefreshCommand ToRefreshCommand(this Proto.V1RefreshRequest requestProto)
    {
        return new RefreshCommand
        {
            UserId = new UserId(requestProto.UserId),
            RefreshToken = requestProto.RefreshToken
        };
    }

    public static Proto.V1RefreshResponse ToV1RefreshResponse(this CommandResponse<RefreshCommandResponse> commandResponse)
    {
        return commandResponse.Match(
            success => new Proto.V1RefreshResponse { SuccessResponse = ToSuccessResponse(success) },
            validationError => new Proto.V1RefreshResponse
            {
                ValidationError = validationError.ToProto<RefreshCommand, Proto.V1RefreshRequest>()
            },
            otherError => new Proto.V1RefreshResponse { OtherError = otherError.ToProto() });

        static Proto.V1RefreshResponse.Types.Success ToSuccessResponse(RefreshCommandResponse commandSuccess)
        {
            return new Proto.V1RefreshResponse.Types.Success
            {
                Token = commandSuccess.AuthToken.ToTokenProto()
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

    public static VerifyEmailCommand ToVerifyEmailCommand(this Proto.V1VerifyEmailRequest requestProto)
    {
        return new VerifyEmailCommand
        {
            Token = requestProto.Token
        };
    }

    public static Proto.V1VerifyEmailResponse ToV1VerifyEmailResponse(this CommandResponse<Success> commandResponse)
    {
        return commandResponse.Match(
            _ => new Proto.V1VerifyEmailResponse { SuccessResponse = new Proto.V1VerifyEmailResponse.Types.Success() },
            validationError => new Proto.V1VerifyEmailResponse
            {
                ValidationError = validationError.ToProto<VerifyEmailCommand, Proto.V1VerifyEmailRequest>()
            },
            otherError => new Proto.V1VerifyEmailResponse { OtherError = otherError.ToProto() });
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
