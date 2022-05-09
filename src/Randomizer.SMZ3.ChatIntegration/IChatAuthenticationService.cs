using System.Threading;
using System.Threading.Tasks;
using Randomizer.SMZ3.ChatIntegration.Models;

namespace Randomizer.SMZ3.ChatIntegration
{
    public interface IChatAuthenticationService
    {
        Task<string?> GetTokenInteractivelyAsync(CancellationToken cancellationToken);

        Task<ChatUserData?> GetUserData(string accessToken, CancellationToken cancellationToken);
    }
}
