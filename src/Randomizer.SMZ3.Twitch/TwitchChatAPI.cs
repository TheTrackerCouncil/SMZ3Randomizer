using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.ChatIntegration.Models;
using Randomizer.SMZ3.Twitch.Models;

namespace Randomizer.SMZ3.Twitch
{

    public class TwitchChatAPI : IChatApi
    {
        private static readonly JsonSerializerOptions s_serializerOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly HttpClient s_httpClient = new();

//#if DEBUG
//        private const string ClientId = "";
//        private const string ApiEndpoint = "http://localhost:8080/mock/";
//#else
        private const string ClientId = "i8sfdyflu72jddzpaho80fdt6vc3ax";
        private const string ApiEndpoint = "https://api.twitch.tv/helix/";
//#endif

        private string? OAuthToken { get; set; }

        protected ILogger<TwitchChatAPI> _logger { get; }

        public TwitchChatAPI(ILogger<TwitchChatAPI> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse?> MakeApiCallAsync<TRequest, TResponse>(string api, TRequest requestData, HttpMethod method, CancellationToken cancellationToken) where TResponse : TwitchAPIResponse
        {
            var request = GetHttpRequestMessage(api, method);
            request.Content = new StringContent(JsonSerializer.Serialize(requestData, s_serializerOptions), Encoding.UTF8, "application/json");
            return await GetHttpResponseAsync<TResponse>(request, cancellationToken);
        }

        public async Task<T?> MakeApiCallAsync<T>(string api, HttpMethod method, CancellationToken cancellationToken) where T : TwitchAPIResponse
        {
            var request = GetHttpRequestMessage(api, method);
            return await GetHttpResponseAsync<T>(request, cancellationToken);
        }

        public void SetAccessToken(string? token)
        {
            OAuthToken = token;
        }

        private HttpRequestMessage GetHttpRequestMessage(string api, HttpMethod method)
        {
            var request = new HttpRequestMessage(method, ApiEndpoint + api);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", OAuthToken);
            request.Headers.TryAddWithoutValidation("Client-Id", ClientId);
            return request;
        }

        private async Task<T?> GetHttpResponseAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken) where T : TwitchAPIResponse
        {
            HttpResponseMessage response;
            try
            {
                response = await s_httpClient.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Unable to reach out to Twitch API");
                return default;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("{RequestMethod} {RequestUri} returned unexpected {StatusCode} response: {Body}",
                    request.Method.Method, request.RequestUri, (int)response.StatusCode, responseContent);
                return default;
            }

            if (string.IsNullOrEmpty(responseContent))
            {
                _logger.LogError("{RequestMethod} {RequestUri} returned unexpected empty response",
                    request.Method.Method, request.RequestUri);
                return default;
            }

            try
            {
                _logger.LogInformation("{RequestMethod} {RequestUri} returned successful response: {Body}",
                    request.Method.Method, request.RequestUri, responseContent);
                var responseObject = JsonDocument.Parse(responseContent).RootElement.GetProperty("data");

                if (!typeof(T).IsGenericType)
                {
                    var toReturn = JsonSerializer.Deserialize<T>(responseObject[0].GetRawText());
                    if (toReturn != null)
                        toReturn.IsSuccessful = true;
                    return toReturn;
                }
                else
                {
                    var toReturn = JsonSerializer.Deserialize<T>(responseObject.GetRawText());
                    if (toReturn != null)
                        toReturn.IsSuccessful = true;
                    return toReturn;
                }
            }
            catch (JsonException)
            {
                _logger.LogError("Unable to deserialize from {RequestMethod} {RequestUri}. Response: {Body}",
                    request.Method.Method, request.RequestUri, responseContent);
                return default;
            }
        }

        public string GetClientId() => ClientId;
    }
}
