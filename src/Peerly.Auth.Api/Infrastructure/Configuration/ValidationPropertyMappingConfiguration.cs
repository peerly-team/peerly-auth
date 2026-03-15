using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.RefreshAccessToken;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;
using Peerly.Auth.V1;

namespace Peerly.Auth.Api.Infrastructure.Configuration;

public static class ValidationPropertyMappingConfiguration
{
    /// <summary>
    /// Register validation property mapping.
    /// As we do validation on business types it means that we should map types in the following way:
    /// <![CDATA[ AddMapping<SourceType, DestinationType>() ]]>
    /// Where SourceType is our business model type
    /// and DestinationType is proto request type
    /// </summary>
    public static void Configure()
    {
        ValidationPropertyMapping
            .AddMapping<LoginCommand, V1LoginRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<RegisterCommand, V1RegisterRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<RefreshCommand, V1RefreshRequest>()
            .Build();

        ValidationPropertyMapping
            .AddMapping<LogoutCommand, V1LogoutRequest>()
            .Build();
    }
}
