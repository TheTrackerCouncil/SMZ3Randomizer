using Microsoft.Extensions.DependencyInjection;
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

        services.AddSingleton<IMetadataService, MetadataService>();

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.Bosses;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.Items;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.Locations;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.Regions;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.Requests;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.Responses;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.Rooms;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.Rewards;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.UILayouts;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.GameLines;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.MsuConfig;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.HintTiles;
        });

        services.AddScoped(serviceProvider =>
        {
            var configs = serviceProvider.GetRequiredService<IMetadataService>();
            return configs.Metadata;
        });

        return services;
    }
}
