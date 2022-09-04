using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.ChatIntegration.Models;
using Randomizer.SMZ3.Twitch.Models;

namespace Randomizer.SMZ3.Twitch
{
    public class TwitchAuthenticationService : OAuthChatAuthenticationService
    {
        private const string ValidateTokenEndpoint = "https://id.twitch.tv/oauth2/validate";

        private static readonly HttpClient s_httpClient = new();

        private readonly IChatApi _twitchChatAPI;
        private readonly ILogger<TwitchAuthenticationService> _logger;

        public TwitchAuthenticationService(IChatApi twitchChatAPI,
            ILogger<TwitchAuthenticationService> logger)
        {
            _twitchChatAPI = twitchChatAPI;
            _logger = logger;
        }

        public override Uri GetOAuthUrl(Uri redirectUri)
        {
            return new Uri("https://id.twitch.tv/oauth2/authorize" +
                $"?client_id={_twitchChatAPI.GetClientId()}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri.ToString())}" +
                "&response_type=token" +
                $"&scope={Uri.EscapeDataString("chat:read chat:edit channel:moderate channel:read:polls channel:manage:polls channel:read:predictions channel:manage:predictions")}");
        }

        public override async Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(ValidateTokenEndpoint));
                request.Headers.Authorization = new("OAuth", accessToken);

                var response = await s_httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var validateResponse = await JsonSerializer.DeserializeAsync<ValidateResponse>(responseStream, cancellationToken: cancellationToken);
                    if (validateResponse?.ExpiresIn > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while checking Twitch OAuth token validity.");
            }

            return false;
        }

        public async Task<bool> RevokeTokenAsync(string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while revoking Twitch OAuth token.");
            }

            return false;
        }

        public override async Task<AuthenticatedUserData?> GetAuthenticatedUserDataAsync(string accessToken, CancellationToken cancellationToken)
        {
            _twitchChatAPI.SetAccessToken(accessToken);
            var user = await _twitchChatAPI.MakeApiCallAsync<TwitchUser>("users", HttpMethod.Get, cancellationToken);
            return user == null ? null : new()
            {
                Name = user.Login,
                DisplayName = user.DisplayName,
                Id = user.Id
            };
        }
    }
}
