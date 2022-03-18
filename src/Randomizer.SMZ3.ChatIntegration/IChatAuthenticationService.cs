using System.Threading;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.ChatIntegration
{
    public interface IChatAuthenticationService
    {
        Task<string?> GetTokenInteractivelyAsync(CancellationToken cancellationToken);

        Task<string?> GetUserNameAsync(string accessToken, CancellationToken cancellationToken);
    }
}
