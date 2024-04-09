using System.Threading.Tasks;
using Randomizer.Data.GeneratedData;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Interfaces;

public interface IRomGenerationService
{
    public SeedData GeneratePlandoSeed(RandomizerOptions options, PlandoConfig plandoConfig);
    public Task<GeneratedRomResult> GenerateRandomRomAsync(RandomizerOptions options, int attempts = 5);
    public byte[] GenerateRomBytes(RandomizerOptions options, SeedData? seed);

    public Task<GeneratedRomResult> GeneratePlandoRomAsync(RandomizerOptions options, PlandoConfig plandoConfig);

    public Task<GeneratedRomResult> GeneratePreSeededRomAsync(RandomizerOptions options, SeedData seed,
        MultiplayerGameDetails multiplayerGameDetails);
}
