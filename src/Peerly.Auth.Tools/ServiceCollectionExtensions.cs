using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Scrutor;

namespace Peerly.Auth.Tools;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static ILifetimeSelector AddNonGenericImplementationsOf(
        this IImplementationTypeSelector implementationTypeSelector,
        Type genericInterfaceType,
        Type[]? excludedImplementationTypes = null,
        RegistrationStrategy? registrationStrategy = null)
    {
        ArgumentNullException.ThrowIfNull(implementationTypeSelector);
        ArgumentNullException.ThrowIfNull(genericInterfaceType);

        if (genericInterfaceType is not { IsInterface: true, IsGenericTypeDefinition: true })
            throw new ArgumentException("Should be generic interface type definition.", nameof(genericInterfaceType));

        var lifetimeSelector = implementationTypeSelector
            .AddClasses(
                classes => classes
                    .AssignableTo(genericInterfaceType)
                    .Where(classType => classType is { IsGenericType: false, IsNestedPrivate: false })
                    .Where(classType => excludedImplementationTypes?.Contains(classType) is not true),
                publicOnly: false)
            .UsingRegistrationStrategy(registrationStrategy ?? RegistrationStrategy.Throw)
            .AsImplementedInterfaces(
                interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericInterfaceType);

        return lifetimeSelector;
    }
}
