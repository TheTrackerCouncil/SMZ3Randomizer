using System.Threading.Tasks;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.Interfaces;

public interface IRomGenerationService
{
    public SeedData GeneratePlandoSeed(RandomizerOptions options, PlandoConfig plandoConfig);
    public Task<GeneratedRomResult> GenerateRandomRomAsync(RandomizerOptions options, int attempts = 5);
    public byte[] GenerateRomBytes(RandomizerOptions options, SeedData? seed, ParsedRomDetails? parsedRomDetails);
    public Task<GeneratedRomResult> GeneratePlandoRomAsync(RandomizerOptions options, PlandoConfig plandoConfig);
    public Task<GeneratedRomResult> GenerateParsedRomAsync(RandomizerOptions options, ParsedRomDetails parsedRomDetails);
    public Task<GeneratedRomResult> GeneratePreSeededRomAsync(RandomizerOptions options, SeedData seed,
        MultiplayerGameDetails multiplayerGameDetails);

    public void ApplyCasPatches(byte[] rom, PatchOptions options);
}
