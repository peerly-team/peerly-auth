using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Api.Controllers.Auth;
using Peerly.Auth.Api.Extensions;
using Peerly.Auth.Api.Infrastructure.Configuration;
using Peerly.Auth.ApplicationServices.Extensions;
using Peerly.Auth.Persistence.Extensions;

namespace Peerly.Auth.Hosting;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureGrpc(builder);
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        RegistrationEndpoints(app);

        await app.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Api
        services.ConfigureApi(configuration);

        // ApplicationServices
        services.ConfigureApplicationServices(configuration);

        // Persistence
        services.ConfigurePersistence(configuration);
    }

    private static void ConfigureGrpc(WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();

        builder.WebHost.ConfigureKestrel(
            o =>
            {
                o.ListenLocalhost(
                    5002,
                    lo =>
                    {
                        lo.UseHttps();
                        lo.Protocols = HttpProtocols.Http2;
                    });
            });
    }

    private static void RegistrationEndpoints(WebApplication app)
    {
        app.UseRouting();

        app.MapGrpcService<AuthController>();

        app.MapGrpcReflectionService();

        // infrastructure configuration
        ValidationPropertyMappingConfiguration.Configure();
    }
}
