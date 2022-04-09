using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.ChatIntegration;

namespace Randomizer.SMZ3.Twitch
{
    public class TwitchAuthenticationService : OAuthChatAuthenticationService
    {
        private const string ClientId = "i8sfdyflu72jddzpaho80fdt6vc3ax";

        private static readonly HttpClient s_httpClient = new();
        private readonly ILogger<TwitchAuthenticationService> _logger;

        public TwitchAuthenticationService(ILogger<TwitchAuthenticationService> logger)
        {
            _logger = logger;
        }

        public override Uri GetOAuthUrl(Uri redirectUri)
        {
            return new Uri("https://id.twitch.tv/oauth2/authorize" +
                $"?client_id={ClientId}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri.ToString())}" +
                "&response_type=token" +
                $"&scope={Uri.EscapeDataString("chat:read chat:edit channel:moderate")}");
        }

        public override async Task<string?> GetUserNameAsync(string accessToken, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitch.tv/helix/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.TryAddWithoutValidation("Client-Id", ClientId);

            var response = await s_httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("{RequestMethod} {RequestUri} returned unexpected {StatusCode} response: {Body}",
                    request.Method.Method, request.RequestUri, (int)response.StatusCode, responseContent);
                return null;
            }

            var responseObject = JsonDocument.Parse(responseContent);
            var user = responseObject.RootElement.GetProperty("data")[0];
            return user.GetProperty("login").GetString();
        }
    }
}
