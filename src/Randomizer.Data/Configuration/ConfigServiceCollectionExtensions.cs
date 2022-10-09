using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Randomizer.Data.Services;

namespace Randomizer.Data.Configuration
{
    public static class ConfigServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigs(this IServiceCollection services)
        {
            services.AddSingleton<ConfigProvider>();
            services.AddTransient(serviceProvider =>
            {
                var configProvider = serviceProvider.GetRequiredService<ConfigProvider>();
                return configProvider.GetMapConfig();
            });

            services.AddTransient<Configs>();
            services.AddTransient<IMetadataService, MetadataService>();

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.Bosses;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.Dungeons;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.Items;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.Locations;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.Regions;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.Requests;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.Responses;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.Rooms;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.Rewards;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.UILayouts;
            });

            services.AddScoped(serviceProvider =>
            {
                var configs = serviceProvider.GetRequiredService<Configs>();
                return configs.GameLines;
            });

            return services;
        }
    }
}
