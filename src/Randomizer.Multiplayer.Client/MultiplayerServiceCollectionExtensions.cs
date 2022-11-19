using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Randomizer.Multiplayer.Client.GameServices;

namespace Randomizer.Multiplayer.Client;

public static class MultiplayerServiceCollectionExtensions
{
    public static IServiceCollection AddMultiplayerServices(this IServiceCollection services)
    {
        services.AddSingleton<MultiplayerClientService>();
        services.AddScoped<MultiplayerGameService>();
        services.AddGameTypeServices<MultiplayerGameService>();
        return services;
    }

    private static IServiceCollection AddGameTypeServices<TAssembly>(this IServiceCollection services)
    {
        var moduleTypes = typeof(TAssembly).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(MultiplayerGameTypeService)));

        foreach (var moduleType in moduleTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(MultiplayerGameTypeService), moduleType));
        }

        return services;
    }
}
