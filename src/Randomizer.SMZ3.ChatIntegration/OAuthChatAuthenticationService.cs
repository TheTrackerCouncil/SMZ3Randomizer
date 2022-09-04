using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Randomizer.SMZ3.ChatIntegration.Models;

namespace Randomizer.SMZ3.ChatIntegration
{
    public abstract class OAuthChatAuthenticationService : IChatAuthenticationService
    {
        public abstract Uri GetOAuthUrl(Uri redirectUri);

        public virtual async Task<string?> GetTokenInteractivelyAsync(CancellationToken cancellationToken)
        {
            var stoppingToken = new CancellationTokenSource();
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken.Token, cancellationToken);

            string? accessToken = null;
            try
            {
                var server = new WebHostBuilder()
                    .UseKestrel(options => options.ListenLocalhost(42069))
                    .Configure(app =>
                    {
                        app.Run(async context =>
                        {
                            if (context.Request.Method == "GET")
                            {
                                var response = @"
<script>
    fetch('?' + document.location.hash.slice(1), { method: 'POST' })
        .then(response => {
            document.write(response.ok ? 'You may now close this window.' : 'Oops. Something went wrong.');
            window.close();
        });
</script>";
                                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(response));
                            }
                            else if (context.Request.Method == "POST")
                            {
                                accessToken = context.Request.Query["access_token"];
                                context.Response.StatusCode = accessToken != null ? 200 : 400;
                                await context.Response.Body.FlushAsync();
                                stoppingToken.CancelAfter(1000);
                            }
                            else
                            {
                                context.Response.StatusCode = 405;
                            }
                        });
                    })
                    .Build();

                var authUrl = GetOAuthUrl(new Uri("http://localhost:42069"));
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl.ToString(),
                    UseShellExecute = true
                });

                await server.RunAsync(combinedToken.Token);
            }
            catch (OperationCanceledException) { }

            return accessToken;
        }

        public abstract Task<AuthenticatedUserData?> GetAuthenticatedUserDataAsync(string accessToken, CancellationToken cancellationToken);
        public abstract Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken);
        public abstract Task<bool> RevokeTokenAsync(string accessToken, CancellationToken cancellationToken);
    }
}
