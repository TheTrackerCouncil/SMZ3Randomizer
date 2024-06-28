using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using TrackerCouncil.Smz3.Chat.Integration;
using TrackerCouncil.Smz3.Chat.Integration.Models;
using TrackerCouncil.Smz3.Chat.Twitch.Models;

namespace TrackerCouncil.Smz3.Chat.Twitch;

public class TwitchAuthenticationService(
    IChatApi twitchChatAPI,
    ILogger<TwitchAuthenticationService> logger)
    : OAuthChatAuthenticationService
{
    private const string ValidateTokenEndpoint = "https://id.twitch.tv/oauth2/validate";
    private const string RevokeTokenEndpoint = "https://id.twitch.tv/oauth2/revoke";

    private static readonly HttpClient s_httpClient = new();

    public override Uri GetOAuthUrl(Uri redirectUri)
    {
        return new Uri("https://id.twitch.tv/oauth2/authorize" +
                       $"?client_id={twitchChatAPI.GetClientId()}" +
                       $"&redirect_uri={Uri.EscapeDataString(redirectUri.ToString())}" +
                       "&response_type=token" +
                       $"&scope={Uri.EscapeDataString("chat:read chat:edit channel:moderate channel:read:polls channel:manage:polls channel:read:predictions channel:manage:predictions moderator:manage:announcements")}");
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
            logger.LogError(ex, "An unexpected error occurred while checking Twitch OAuth token validity.");
        }

        return false;
    }

    public override async Task<bool> RevokeTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(RevokeTokenEndpoint));
            request.Content = new FormUrlEncodedContent(new[]{
                new KeyValuePair<string?, string?>("client_id", twitchChatAPI.GetClientId()),
                new KeyValuePair<string?, string?>("token", accessToken)
            });

            var response = await s_httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Revoked Twitch access token successfully.");
                return true;
            }

            using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var apiResponse = await JsonSerializer.DeserializeAsync<TwitchAPIResponse>(responseStream, cancellationToken: cancellationToken);
            if (apiResponse == null)
            {
                logger.LogError("Unable to revoke Twitch token, and Twitch returned an invalid response.");
            }
            else
            {
                logger.LogError("Unable to revoke Twitch token: Twitch returned {Status} with message '{Message}'",
                    apiResponse.Status, apiResponse.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while revoking Twitch OAuth token.");
        }

        return false;
    }

    public override async Task<AuthenticatedUserData?> GetAuthenticatedUserDataAsync(string accessToken, CancellationToken cancellationToken)
    {
        twitchChatAPI.SetAccessToken(accessToken);
        var user = await twitchChatAPI.MakeApiCallAsync<TwitchUser>("users", HttpMethod.Get, cancellationToken);
        return user == null ? null : new()
        {
            Name = user.Login,
            DisplayName = user.DisplayName,
            Id = user.Id
        };
    }
}
