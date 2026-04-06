using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Messaging.Options;
using Peerly.Auth.Messaging.Producers.UserRegistrationEvents;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.Messaging.Producers;

[ExcludeFromCodeCoverage]
internal sealed class KafkaProducerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services
            .AddOptions<KafkaProducerOptions>()
            .BindConfiguration(KafkaProducerOptions.SectionName);

        services.AddHostedService<UserRegistrationEventsPublisher>();
    }
}
