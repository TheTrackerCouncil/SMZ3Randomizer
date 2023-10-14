using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.FileData.Patches;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RandomizerServiceCollectionExtensions
    {
        public static IServiceCollection AddRandomizerServices(this IServiceCollection services)
        {
            services.AddRomPatches<RomPatchFactory>();
            services.AddSingleton<RomPatchFactory>();
            services.AddGeneratedRomLoader();
            services.AddSmz3Randomizer();
            services.AddPlandomizer();
            services.AddSmz3MultiplayerRomGenerator();
            services.AddSingleton<RomGenerationService>();
            services.AddSingleton<SpritePatcherService>();
            services.AddSingleton<RomTextService>();
            services.AddTransient<RomLauncherService>();
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
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(RomPatch), moduleType));
            }

            return services;
        }
    }
}
