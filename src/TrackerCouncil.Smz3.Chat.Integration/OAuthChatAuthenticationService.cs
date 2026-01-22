using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using TrackerCouncil.Smz3.Chat.Integration.Models;

namespace TrackerCouncil.Smz3.Chat.Integration;

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
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(42069);
            });

            var app = builder.Build();

            app.MapGet("/", async (context) =>
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
            });

            app.MapPost("/", async (context) =>
            {
                accessToken = context.Request.Query["access_token"];
                context.Response.StatusCode = !string.IsNullOrEmpty(accessToken)
                    ? StatusCodes.Status200OK
                    : StatusCodes.Status400BadRequest;
                await context.Response.Body.FlushAsync(combinedToken.Token);

                stoppingToken.CancelAfter(1000);
            });

            var authUrl = GetOAuthUrl(new Uri("http://localhost:42069"));
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl.ToString(),
                UseShellExecute = true
            });

            await app.StartAsync(combinedToken.Token);
            await app.WaitForShutdownAsync(combinedToken.Token);
        }
        catch (OperationCanceledException) { }

        return accessToken;
    }

    public abstract Task<AuthenticatedUserData?> GetAuthenticatedUserDataAsync(string accessToken, CancellationToken cancellationToken);
    public abstract Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken);
    public abstract Task<bool> RevokeTokenAsync(string accessToken, CancellationToken cancellationToken);
}
