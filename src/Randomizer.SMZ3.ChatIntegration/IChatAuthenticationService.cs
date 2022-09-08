using System.Threading;
using System.Threading.Tasks;
using Randomizer.SMZ3.ChatIntegration.Models;

namespace Randomizer.SMZ3.ChatIntegration
{
    public interface IChatAuthenticationService
    {
        Task<string?> GetTokenInteractivelyAsync(CancellationToken cancellationToken);

        Task<AuthenticatedUserData?> GetAuthenticatedUserDataAsync(string accessToken, CancellationToken cancellationToken);
        Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken);
        Task<bool> RevokeTokenAsync(string accessToken, CancellationToken cancellationToken);
    }
}
