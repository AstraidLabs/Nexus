using Microsoft.Extensions.DependencyInjection;
using Nexus.Core.Abstractions;
using Nexus.Core.Services;
using System;

namespace Nexus.DependencyInjection;

/// <summary>
/// Extension methods for registering Nexus services into an <see cref="IServiceCollection"/>.
/// </summary>
public static class NexusServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Nexus API and all required dependencies to the service collection.
    /// </summary>
    public static IServiceCollection AddNexus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IWindowsSystemReader, WindowsSystemReader>();
        services.AddSingleton<IKmsClientReader, KmsClientReader>();
        services.AddSingleton<IAdvancedActivationReader, AdvancedActivationReader>();
        services.AddSingleton<ISubscriptionReader, SubscriptionReader>();
        services.AddSingleton<ISkuReader, SkuReader>();
        services.AddSingleton<IWindowsInfoFacade, WindowsInfoFacade>();
        services.AddSingleton<NexusAPI>();

        return services;
    }
}
