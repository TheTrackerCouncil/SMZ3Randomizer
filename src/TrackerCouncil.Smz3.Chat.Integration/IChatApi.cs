using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TrackerCouncil.Smz3.Chat.Integration.Models;

namespace TrackerCouncil.Smz3.Chat.Integration;

public interface IChatApi
{
    Task<T?> MakeApiCallAsync<T>(string api, HttpMethod method, CancellationToken cancellationToken) where T : TwitchAPIResponse;
    Task<TResponse?> MakeApiCallAsync<TRequest, TResponse>(string api, TRequest requestData, HttpMethod method, CancellationToken cancellationToken) where TResponse : TwitchAPIResponse;
    void SetAccessToken(string? authToken);
    string GetClientId();
}
