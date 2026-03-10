using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.Tools;

public static class InstallerExtensions
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InstallServicesFromExecutingAssembly(
        this IServiceCollection services,
        IConfiguration configuration,
        bool publicOnly = false)
    {
        // Получаем сборку, из которой был вызван текущий метод
        var assembly = Assembly.GetCallingAssembly();

        var installers = GetInstallers<IInstaller>(assembly, publicOnly);
        foreach (var installer in installers)
        {
            installer.InstallServices(services);
        }

        var configurableInstallers = GetInstallers<IConfigurableInstaller>(assembly, publicOnly);
        foreach (var installer in configurableInstallers)
        {
            installer.InstallServices(services, configuration);
        }
    }

    private static IEnumerable<TInstaller> GetInstallers<TInstaller>(Assembly assembly, bool publicOnly)
    {
        return assembly
            .GetTypes()
            .Where(
                type =>
                    type is { IsInterface: false, IsAbstract: false }
                    && (!publicOnly || type.IsPublic)
                    && typeof(TInstaller).IsAssignableFrom(type))
            .Select(Activator.CreateInstance)
            .Cast<TInstaller>();
    }
}
