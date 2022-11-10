using Microsoft.AspNetCore.SignalR;

namespace Randomizer.Multi.Server
{
    public class MultiworldHub : Hub
    {
        public async Task CreateGame()
        {
            Console.WriteLine("test");
        }
    }
}
