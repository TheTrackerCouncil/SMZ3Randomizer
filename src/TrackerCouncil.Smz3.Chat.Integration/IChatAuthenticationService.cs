using System.Threading;
using System.Threading.Tasks;
using TrackerCouncil.Smz3.Chat.Integration.Models;

namespace TrackerCouncil.Smz3.Chat.Integration;

public interface IChatAuthenticationService
{
    Task<string?> GetTokenInteractivelyAsync(CancellationToken cancellationToken);

    Task<AuthenticatedUserData?> GetAuthenticatedUserDataAsync(string accessToken, CancellationToken cancellationToken);
    Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken);
    Task<bool> RevokeTokenAsync(string accessToken, CancellationToken cancellationToken);
}
