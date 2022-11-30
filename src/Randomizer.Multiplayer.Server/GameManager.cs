using Microsoft.AspNetCore.SignalR;

namespace Randomizer.Multiplayer.Server;

public class GameManager
{
    private readonly ILogger<GameManager> _logger;
    private IHubContext<MultiplayerHub> _hubContext;
    private readonly int _expirationMinutes;
    private readonly int _checkFrequencyMinutes;

    public GameManager(ILogger<GameManager> logger, IHubContext<MultiplayerHub> hubContext, IConfiguration configuration)
    {
        _logger = logger;
        _hubContext = hubContext;

        _expirationMinutes = configuration.GetValue("SMZ3:GameMemoryExpirationInMinutes", 60);
        _checkFrequencyMinutes = configuration.GetValue("SMZ3:GameCheckFrequencyInMinutes", 60);

        Task.Run(CheckGames);
    }


    /// <summary>
    /// Checks games to see if they need to be expired/deleted
    /// </summary>
    private async Task CheckGames()
    {
        while (true)
        {
            var amountExpired = MultiplayerGame.ExpireGames(_expirationMinutes);
            if (amountExpired > 0)
            {
                _logger.LogInformation("Expired {Amount} inactive games(s)", amountExpired);
            }
            await Task.Delay(TimeSpan.FromMinutes(_checkFrequencyMinutes));
        }
    }

}
