using System;
using System.IO;
using System.Linq;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class RomParserService
{
    public void ParseRomFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException();
        }

        var world = new World(new Config(), "", 1, "");
        var rom = File.ReadAllBytes(filePath);
        var multiworld = MetadataPatch.IsRomMultiworldEnabled(rom);
        var keysanity = MetadataPatch.IsRomKeysanityEnabled(rom);
        var players = MetadataPatch.GetPlayerNames(rom);
        var playerIndex = MetadataPatch.GetPlayerIndex(rom);
        var romTitle = MetadataPatch.GetGameTitle(rom);
        var medallions = MedallionPatch.GetRequiredMedallions(rom);
        ZeldaRewardsPatch.ApplyRewardsFromRom(rom, world);
        var hardLogic = false;

        if (romTitle.StartsWith("ZSM"))
        {
            var flags = romTitle[3..];
            var flagIndex = flags.IndexOf(flags.FirstOrDefault(char.IsLetter));
            flags = flags.Substring(flagIndex, 2);
            hardLogic = flags[1] == 'H';
        }

        var playerName = players[playerIndex];
        var locations = LocationsPatch.GetLocationsFromRom(rom, players);
    }
}
