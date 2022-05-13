using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.ChatIntegration.Models;
using Randomizer.SMZ3.Twitch.Models;

namespace Randomizer.SMZ3.Twitch
{
    public class TwitchAuthenticationService : OAuthChatAuthenticationService
    {
        private readonly IChatApi _twitchChatAPI;

        public TwitchAuthenticationService(IChatApi twitchChatAPI)
        {
            _twitchChatAPI = twitchChatAPI;
        }

        public override Uri GetOAuthUrl(Uri redirectUri)
        {
            return new Uri("https://id.twitch.tv/oauth2/authorize" +
                $"?client_id={_twitchChatAPI.GetClientId()}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri.ToString())}" +
                "&response_type=token" +
                $"&scope={Uri.EscapeDataString("chat:read chat:edit channel:moderate channel:read:polls channel:manage:polls channel:read:predictions channel:manage:predictions")}");
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
