using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrackerCouncil.Smz3.Data.Interfaces;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;
using TrackerCouncil.Smz3.SeedGenerator.Generation;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator;

public static class RandomizerServiceCollectionExtensions
{
    public static IServiceCollection AddRandomizerServices(this IServiceCollection services)
    {
        services.AddRomPatches<RomPatchFactory>();
        services.AddSingleton<RomPatchFactory>();
        services.AddGeneratedRomLoader();
        services.AddSmz3Randomizer();
        services.AddPlandomizer();
        services.AddRomParser();
        services.AddSmz3MultiplayerRomGenerator();
        services.AddSingleton<IRomGenerationService, RomGenerationService>();
        services.AddSingleton<SpritePatcherService>();
        services.AddSingleton<RomTextService>();
        services.AddTransient<RomLauncherService>();
        services.AddTransient<PlaythroughService>();
        services.AddTransient<IStatGenerator, StatGenerator>();
        return services;
    }

    private static IServiceCollection AddSmz3Randomizer(this IServiceCollection services)
    {
        services.AddTransient<IGameHintService, GameHintService>();
        services.AddTransient<IPatcherService, PatcherService>();
        services.AddSingleton<RandomizerContext>();
        services.AddSingleton<IFiller, StandardFiller>();
        services.AddSingleton<IWorldAccessor, WorldAccessor>();
        services.AddSingleton<Smz3Randomizer>();
        return services;
    }

    private static IServiceCollection AddPlandomizer(this IServiceCollection services)
    {
        services.AddSingleton<IPlandoConfigLoader, PlandoConfigLoader>();
        services.AddSingleton<PlandoFillerFactory>();
        services.AddSingleton<Smz3Plandomizer>();
        return services;
    }

    private static IServiceCollection AddRomParser(this IServiceCollection services)
    {
        services.AddSingleton<Smz3RomParser>();
        services.AddSingleton<ParsedRomFiller>();
        return services;
    }

    private static IServiceCollection AddGeneratedRomLoader(this IServiceCollection services)
    {
        services.AddSingleton<Smz3GeneratedRomLoader>();
        return services;
    }

    private static IServiceCollection AddSmz3MultiplayerRomGenerator(this IServiceCollection services)
    {
        services.AddSingleton<MultiplayerFillerFactory>();
        services.AddSingleton<Smz3MultiplayerRomGenerator>();
        return services;
    }

    private static IServiceCollection AddRomPatches<TAssembly>(this IServiceCollection services)
    {
        var moduleTypes = typeof(TAssembly).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(RomPatch)));

        foreach (var moduleType in moduleTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(RomPatch), moduleType));
        }

        return services;
    }
}
