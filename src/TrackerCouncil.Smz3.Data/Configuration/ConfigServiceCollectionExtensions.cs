﻿using Microsoft.Extensions.DependencyInjection;
using TrackerCouncil.Smz3.Data.Services;

namespace TrackerCouncil.Smz3.Data.Configuration;

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

        services.AddSingleton<Configs>();
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

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<Configs>();
            return configs.MsuConfig;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<Configs>();
            return configs.HintTileConfig;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<Configs>();
            return configs.MetadataConfig;
        });

        return services;
    }
}
