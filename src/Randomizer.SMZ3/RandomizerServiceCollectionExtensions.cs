using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RandomizerServiceCollectionExtensions
    {
        public static IServiceCollection AddSmz3Randomizer(this IServiceCollection services)
        {
            services.AddTransient<IGameHintService, GameHintService>();
            services.AddSingleton<RandomizerContext>();
            services.AddSingleton<IFiller, StandardFiller>();
            services.AddSingleton<IWorldAccessor, WorldAccessor>();
            services.AddSingleton<Smz3Randomizer>();
            return services;
        }

        public static IServiceCollection AddPlandomizer(this IServiceCollection services)
        {
            services.AddSingleton<IPlandoConfigLoader, PlandoConfigLoader>();
            services.AddSingleton<PlandoFillerFactory>();
            services.AddSingleton<Smz3Plandomizer>();
            return services;
        }

        public static IServiceCollection AddGeneratedRomLoader(this IServiceCollection services)
        {
            services.AddSingleton<Smz3GeneratedRomLoader>();
            return services;
        }

        public static IServiceCollection AddSmz3MultiplayerRomGenerator(this IServiceCollection services)
        {
            services.AddSingleton<MultiplayerFillerFactory>();
            services.AddSingleton<Smz3MultiplayerRomGenerator>();
            return services;
        }
    }
}
