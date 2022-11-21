using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client.GameServices;

public abstract class MultiplayerGameTypeService
{

    public MultiplayerGameTypeService(Smz3Randomizer randomizer)
    {
        Randomizer = randomizer;
    }

    protected Smz3Randomizer Randomizer { get; init; }

    public abstract SeedData? GenerateSeed(List<MultiplayerPlayerState> players, out string error);

    public abstract SeedData? RegenerateSeed(List<MultiplayerPlayerState> players, MultiplayerPlayerState localPlayer, string seed, out string error);

    public MultiplayerPlayerState GetPlayerDefaultState(MultiplayerPlayerState state, World world)
    {
        state.Locations = world.Locations.ToDictionary(x => x.Id, x => false);
        state.Items = world.LocationItems.Select(x => x.Type).Distinct().ToDictionary(x => x, _ => 0);
        state.Bosses = world.GoldenBosses.Select(x => x.Type).ToDictionary(x => x, _ => false);
        state.Dungeons = world.Dungeons.ToDictionary(x => x.DungeonName, _ => false);
        return state;
    }

    protected IEnumerable<Config> GetPlayerConfigs(List<MultiplayerPlayerState> players)
        => players.Select(x => Config.FromConfigString(x.Config!).First());

    protected SeedData? GenerateSeedInternal(List<Config> configs, string? seed, out string error)
    {
        SeedData? seedData = null;
        var validated = false;
        error = "";
        for (var i = 0; i < 3; i++)
        {
            try
            {
                seedData = Randomizer.GenerateSeed(configs, seed, CancellationToken.None);
                if (!Randomizer.ValidateSeedSettings(seedData))
                {
                    error = "";
                }
                else
                {
                    validated = true;
                    break;
                }
            }
            catch (RandomizerGenerationException e)
            {
                seedData = null;
                error = $"Error generating rom\n{e.Message}\nPlease try again. If it persists, try modifying your seed settings.";
            }
        }

        if (!validated)
        {
            error = $"Could not successfully generate a seed with requested settings.";
            seedData = null;
        }

        return seedData;
    }

    public string GetValidationHash(IEnumerable<World> worlds)
    {
        var itemHashCode = string.Join(",",
            worlds.SelectMany(x => x.Locations).OrderBy(x => x.World.Id).ThenBy(x => x.Id)
                .Select(x => x.Item.Type.ToString()));
        var rewardHashCode = string.Join(",",
            worlds.SelectMany(x => x.Regions).OrderBy(x => x.World.Id)
                .ThenBy(x => x.Name)
                .OfType<IHasReward>().Select(x => x.RewardType.ToString()));
        return $"{NonCryptographicHash.Fnv1a(itemHashCode)}{NonCryptographicHash.Fnv1a(rewardHashCode)}";
    }
}
