using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Peerly.Auth.Api.Controllers.Auth;
using Peerly.Auth.Api.Extensions;
using Peerly.Auth.Api.Infrastructure.Configuration;
using Peerly.Auth.ApplicationServices.Extensions;
using Peerly.Auth.IntegrationTests.Features.V1.Auth.ConfirmEmail.Infrastructure;
using Peerly.Auth.IntegrationTests.Features.V1.Auth.Login.Infrastructure;
using Peerly.Auth.IntegrationTests.Features.V1.Auth.Logout.Infrastructure;
using Peerly.Auth.Persistence.Extensions;

namespace Peerly.Auth.IntegrationTests.Infrastructure;

public sealed class WebApplicationFactory : IAsyncDisposable
{
    private readonly string _databaseHost;
    private readonly int _databasePort;
    private readonly string _databaseName;
    private readonly string _databaseUsername;
    private readonly string _databasePassword;
    private IHost? _host;

    public WebApplicationFactory(
        string databaseHost,
        int databasePort,
        string databaseName,
        string databaseUsername,
        string databasePassword)
    {
        _databaseHost = databaseHost;
        _databasePort = databasePort;
        _databaseName = databaseName;
        _databaseUsername = databaseUsername;
        _databasePassword = databasePassword;
    }

    public async Task StartAsync()
    {
        _host = await Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                (_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(GetConfiguration());
                })
            .ConfigureWebHostDefaults(
                webBuilder =>
                {
                    webBuilder.UseEnvironment("IntegrationTests");
                    webBuilder.UseTestServer();
                    webBuilder.ConfigureServices(ConfigureServices);
                    webBuilder.Configure(ConfigureApplication);
                })
            .StartAsync();
    }

    public ConfirmEmailGrpcClient CreateConfirmEmailClient()
    {
        return new ConfirmEmailGrpcClient(CreateGrpcChannel());
    }

    public LoginGrpcClient CreateLoginClient()
    {
        return new LoginGrpcClient(CreateGrpcChannel());
    }

    public LogoutGrpcClient CreateLogoutClient()
    {
        return new LogoutGrpcClient(CreateGrpcChannel());
    }

    public IServiceProvider Services => _host?.Services
        ?? throw new InvalidOperationException("Integration test host is not initialized.");

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    private TestServer GetTestServer()
    {
        return _host?.GetTestServer()
            ?? throw new InvalidOperationException("Integration test host is not initialized.");
    }

    private GrpcChannel CreateGrpcChannel()
    {
        var handler = new GrpcWebHandler(GetTestServer().CreateHandler());
        return GrpcChannel.ForAddress(
            "http://localhost",
            new GrpcChannelOptions
            {
                HttpHandler = handler
            });
    }

    private IReadOnlyDictionary<string, string?> GetConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["ExpirationTimeOptions:EmailVerificationTokenValidityPeriodMinutes"] = "10",
            ["ExpirationTimeOptions:AccessTokenPeriodMinutes"] = "15",
            ["ExpirationTimeOptions:RefreshTokenPeriodDays"] = "14",
            ["SigningOptions:SigningKeys:0:PrivateKey"] = GetSigningPrivateKey(),
            ["SigningOptions:SigningKeys:0:Kid"] = "integration-test-key",
            ["SigningOptions:SigningKeys:0:IsActive"] = "true",
            ["ConnectionFactoryOptions:MasterHost"] = _databaseHost,
            ["ConnectionFactoryOptions:DefaultPort"] = _databasePort.ToString(),
            ["ConnectionFactoryOptions:Database"] = _databaseName,
            ["ConnectionFactoryOptions:UserName"] = _databaseUsername,
            ["ConnectionFactoryOptions:Password"] = _databasePassword,
            ["ConnectionFactoryOptions:SslMode"] = "Disable"
        };
    }

    private static void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
    {
        services.AddGrpc();
        services.AddGrpcReflection();

        services.ConfigureApi(context.Configuration);
        services.ConfigureApplicationServices(context.Configuration);
        services.ConfigurePersistence(context.Configuration);

        RemoveHostedServices(services);
    }

    private static void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapGrpcService<AuthController>();
                endpoints.MapGrpcReflectionService();
            });

        ValidationPropertyMappingConfiguration.Configure();
    }

    private static void RemoveHostedServices(IServiceCollection services)
    {
        var hostedServices = services
            .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
            .ToArray();

        foreach (var hostedService in hostedServices)
        {
            services.Remove(hostedService);
        }
    }

    private static string GetSigningPrivateKey()
    {
        return """
               -----BEGIN PRIVATE KEY-----
               MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQC/6M1fRGyZwuRm
               iDMfO7CCd5mk0gHDfsGOelF/jNDjbuRRAf8EUSki05RpQGLbJgfLB+aniYNMPbUR
               5pLZAilIJSChPx/VHKnHOsycGcJ7aI7e0ZEXoeHhRWtIXp9p0jCrGJXapO4K5jPu
               7gYHaUCYZ1NWxYKnzcKmP6Q2E0Cr8H7Ss2OJiZ+JaFkXDx7XNEp003xYY4c4jKyh
               aTI1Og4eqNrdfiu/NdOxCetAQSc6eYZ8w+ISuRErU1IIWkHEswIZq5ds/j3Rviyp
               hIvYFM8J2Lf/RrT8tQ5rBzGH3DDP4djjnMb4mt1kNSD094WdioxYiZ5s3Scf7LfT
               Yz2HZipjAgMBAAECggEAEsP1VEitxEzpMv8Q3XYwVYs5/BxU89E9pNL58sD9EOA5
               2/n+ZJh5fBOFKNP7VqnEJVEWd6RCSuUJA0EXc7gUYh7PSM7FX0kFCQHYGoOIuUyJ
               SLYXu/nZmCktMZxr3IsjGysb31w1TERXKH8gTWU4KpVTJUiW9OJxecdk8hMb32dJ
               FPVSP7JcGYJgYz2mrvPl3bL0TG6sxC0rwaUGX74jHWT1WAhF4hsS8ISDgaWE7REt
               invuZojHkwR0jnSh9lvGr7J9YOWkkpYM6mJhSL/78lsGkFq2i1vJNSu5YyeMgxu0
               t8wO0x8+aiHeqh9LlZRfyLkdiIYqfO1Qhud1+RRSkQKBgQDnwnul59F55O/q2GJu
               sMSs1RhmVE6TsyVabeENOD/zOWTlSDXuOCNiEAVGAFXxQCzuNGJIlrUZkvkBv5Iy
               B5DXjNDNfWbFVusEatGUgrXtq/gVGvnnqMwrm5AqSt+vIstJlgyEYehBXntJLtml
               425BPakR4I7VYapTNmFnTjzAnQKBgQDT+00SjTGkyvX7YwqRYeL/4Q1gylOjFKRh
               MGJ7LiwFcD+FBO1OnCyPTWff0oiIkN2MXCLtKfre6mdmx0GEcSXMtd+u8JsB16Na
               FQWfd7TykANGKIcCfLT6vaCJFKZcZxt00En1omxunnEvtPDbVGvxOiDOcao44lT3
               a30YkAcm/wKBgAKB2x6XXG/KJ0JOJvp1lRsdjw8EWHlGld/dknK3KhHyjAHi/xpd
               pxxXegcg180tWY8WJ/4LC1iEe4cmUGmUJV//mP6wHZ2C7DX3Bd9qbpdspdlsmkmE
               TPknzK54cuUNJk/cfLQt7vpOEF1hUV93D2lLRnn4CPOMA/C0hOc+NHANAoGBAJ4x
               0BakIAQnIuLzypMsRcdHIEC3PSta4EFXZmce0eNNHVobjy03B1n6Hia+av3ffjad
               G8N5rKpmq7vbv10jQ1497CwVitgZIOK9BXE4WGUcbBUTcY29myH0GbWzH2Od3rOS
               LV+OUvVKcJV1prlHizZ+drUZxjqlTVtHcBfAhFXpAoGBAI8vseSUlszJ7pIFNNGI
               cOCFiqBJyidBN3jvOksBqONbY6AGx4rlCR/BS5U9Y3WSUKKnoURh4quuomKX8Htz
               JgZE7977OOzBhtzTxSGMvIPPS3BZdp3/Z2wVPkB6oah1riJXJW0JN+dtJ8J0CBQc
               GjVS8iB1Xjl6ZosLyTJjcRRL
               -----END PRIVATE KEY-----
               """;
    }
}
