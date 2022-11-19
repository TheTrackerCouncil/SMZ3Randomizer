using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.Multiplayer.Client.GameServices;
using Randomizer.Shared.Migrations;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public class MultiplayerGameService : IDisposable
{
    private readonly ILogger<MultiplayerGameService> _logger;
    private readonly IEnumerable<MultiplayerGameTypeService> _gameTypeServices;
    private MultiplayerGameTypeService _currentGameService;
    private MultiplayerClientService _client;

    public MultiplayerGameService(MultiplayerClientService clientService, IServiceProvider serviceProvider, ILogger<MultiplayerGameService> logger)
    {
        _gameTypeServices = serviceProvider.GetServices<MultiplayerGameTypeService>();
        _currentGameService = _gameTypeServices.Single(x => x is MultiworldGameService);
        _logger = logger;
        _client = clientService;
    }

    public void UpdateGameType(MultiplayerGameType type)
    {
        _currentGameService = type switch
        {
            _ => _gameTypeServices.Single(x => x is MultiworldGameService)
        };
    }

    public List<MultiplayerPlayerState> CreateWorld(List<MultiplayerPlayerState> players)
    {
        return _currentGameService.CreateWorld(players);
    }

    public void Dispose()
    {
        
    }
}
