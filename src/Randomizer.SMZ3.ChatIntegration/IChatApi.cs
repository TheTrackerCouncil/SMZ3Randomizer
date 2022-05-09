using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.ChatIntegration
{
    public interface IChatApi
    {
        Task<T?> MakeApiCallAsync<T>(string api, HttpMethod method, CancellationToken cancellationToken);
        Task<TResponse?> MakeApiCallAsync<TRequest, TResponse>(string api, TRequest requestData, HttpMethod method, CancellationToken cancellationToken);
        void SetAccessToken(string? authToken);
        string GetClientId();
    }
}
