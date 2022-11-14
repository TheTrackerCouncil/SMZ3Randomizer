using Microsoft.AspNetCore.SignalR;
using Randomizer.Data.Multiworld;

namespace Randomizer.Multiworld.Server
{
    public class MultiworldHub : Hub
    {
        public async Task CreateGame(CreateGameRequest request)
        {
            if (string.IsNullOrEmpty(request.PlayerName))
            {
                await SendErrorResponse<JoinGameResponse>("CreateGame", "Missing required attributes");
                return;
            }

            var results = MultiworldGame.CreateNewGame(request.PlayerName, out var error);

            if (results == null)
            {
                await SendErrorResponse<CreateGameResponse>("CreateGame", error ?? "Unknown error creating game");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, results.Value.Game.Guid);

            await Clients.Caller.SendAsync("CreateGame", new CreateGameResponse()
            {
                IsSuccessful = true,
                GameGuid = results.Value.Game.Guid,
                PlayerGuid = results.Value.Player.Guid,
                PlayerKey = results.Value.Player.Key,
                AllPlayers = results.Value.Game.PlayerStates
            });
        }

        public async Task JoinGame(JoinGameRequest request)
        {
            if (string.IsNullOrEmpty(request.GameGuid) || string.IsNullOrEmpty(request.PlayerName))
            {
                await SendErrorResponse<JoinGameResponse>("JoinGame", "Missing required attributes");
                return;
            }
            var game = MultiworldGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse<JoinGameResponse>("JoinGame", "Unable to find game");
                return;
            }

            var player = game.JoinGame(request.PlayerName, out var error);
            if (player == null)
            {
                await SendErrorResponse<JoinGameResponse>("JoinGame", error ?? "Unknown error joining game");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, request.GameGuid);

            await Clients.Caller.SendAsync("JoinGame", new JoinGameResponse()
            {
                IsSuccessful = true,
                PlayerGuid = player.Guid,
                PlayerKey = player.Key,
                AllPlayers = game.PlayerStates
            });

            await Clients.OthersInGroup(request.GameGuid).SendAsync("PlayerJoined", new PlayerJoinedResponse()
            {
                IsSuccessful = true,
                PlayerGuid = player.Guid,
                PlayerName = player.State.PlayerName,
                AllPlayers = game.PlayerStates
            });
        }

        public async Task SubmitConfig(SubmitConfigRequest request)
        {
            var game = MultiworldGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse<SubmitConfigResponse>("SubmitConfig", "Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true);
            if (player == null)
            {
                await SendErrorResponse<SubmitConfigResponse>("SubmitConfig", "Unable to find player");
                return;
            }

            var configs = game.UpdatePlayerConfig(player, request.Config);

            await Clients.Caller.SendAsync("SubmitConfig", new SubmitConfigResponse() { IsSuccessful = true });

            await Clients.Groups(request.GameGuid).SendAsync("PlayerSync", new PlayerSyncResponse()
            {
                IsSuccessful = true,
                PlayerState = player.State
            });
        }

        public async Task StartGame(StartGameRequest request)
        {
            var game = MultiworldGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse<StartGameResponse>("StartGame", "Unable to find game");
                return;
            }

            var playerStates = game.StartGame(request.PlayerGuids, request.TrackerState);
            await Clients.Group(request.GameGuid).SendAsync("StartGame", new StartGameResponse()
            {
                IsSuccessful = true,
                Players = playerStates
            });
        }

        private async Task SendErrorResponse<T>(string method, string error) where T : MultiworldResponse, new()
        {
            var response = new T() { IsSuccessful = false, Error = error };
            await Clients.Caller.SendAsync(method, response);
        }
    }
}
