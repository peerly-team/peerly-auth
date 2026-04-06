using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Api.Controllers.Auth;
using Peerly.Auth.Api.Extensions;
using Peerly.Auth.Api.Infrastructure.Configuration;
using Peerly.Auth.ApplicationServices.Extensions;
using Peerly.Auth.Messaging.Extensions;
using Peerly.Auth.Persistence.Extensions;

namespace Peerly.Auth.Hosting;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        RegistrationEndpoints(app);

        await app.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpc();
        services.AddGrpcReflection();

        // Api
        services.ConfigureApi(configuration);

        // ApplicationServices
        services.ConfigureApplicationServices(configuration);

        // Messaging
        services.ConfigureMessaging(configuration);

        // Persistence
        services.ConfigurePersistence(configuration);
    }

    private static void RegistrationEndpoints(WebApplication app)
    {
        app.UseRouting();

        app.MapGrpcService<AuthController>();

        app.MapGrpcReflectionService();

        ValidationPropertyMappingConfiguration.Configure();
    }
}
