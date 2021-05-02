using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.Shared.Models;

namespace WebRandomizer {
    public class Program {
        public static async Task Main(string[] args) {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                cts.Cancel();
                e.Cancel = true;
            };

            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope()) {
                var context = scope.ServiceProvider.GetRequiredService<RandomizerContext>();
                context.Database.Migrate();
            }

            await host.StartAsync(cts.Token);
            var addresses = host.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
            if (addresses != null) {
                foreach (var address in addresses) {
                    Console.WriteLine($"Now listening on: {address}");
                }

                LaunchInBrowser(addresses.OrderBy(x => x).Last());
            }

            Console.WriteLine("Application started. Press Ctrl+C to shut down");
            cts.Token.WaitHandle.WaitOne();
            await host.StopAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static void LaunchInBrowser(string url) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Process.Start("open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Process.Start("xdg-open", url);
            }
        }
    }
}
