using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Randomizer.App
{
    internal static class WpfServiceCollectionExtensions
    {
        public static IServiceCollection AddWindows<TAssembly>(this IServiceCollection services)
            => services.AddWindows(typeof(TAssembly).Assembly);

        public static IServiceCollection AddWindows(this IServiceCollection services, Assembly assembly)
        {
            var windows = assembly.GetTypes()
                .Where(x => x.IsAssignableTo(typeof(Window)));
            foreach (var window in windows)
            {
                if (window.GetCustomAttribute<NotAServiceAttribute>() != null)
                    continue;

                services.TryAddScoped(window);
            }

            return services;
        }
    }
}
