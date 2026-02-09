using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SharpYaml.Model;

namespace TrackerCouncil.Smz3.SeedGenerator.Text;

public class StringTable
{
    public const string SahasrahlaReveal = "sahasrahla_quest_information";
    public const string BombShopReveal = "bomb_shop";
    public const string BlindIntro = "blind_by_the_light";
    public const string TavernMan = "kakariko_tavern_fisherman";
    public const string GanonIntro = "ganon_fall_in";
    public const string GanonPhaseThreeText = "ganon_phase_3";
    public const string TriforceRoom = "end_triforce";
    public const string BottleMerchant = "bottle_vendor_choice";
    public const string KingZora = "zora_tells_cost";
    public const string MasterSwordPedestal = "mastersword_pedestal_translated";
    public const string EtherTablet = "tablet_ether_book";
    public const string BombosTablet = "tablet_bombos_book";
    public const string HintTileEasternPalace = "telepathic_tile_eastern_palace";
    public const string HintTileTowerOfHeraFloor4 = "telepathic_tile_tower_of_hera_floor_4";
    public const string HintTileSpectacleRock = "telepathic_tile_spectacle_rock";
    public const string HintTileSwampEntrance = "telepathic_tile_swamp_entrance";
    public const string HintTileThievesTownUpstairs = "telepathic_tile_thieves_town_upstairs";
    public const string HintTileMiseryMire = "telepathic_tile_misery_mire";
    public const string HintTilePalaceOfDarkness = "telepathic_tile_palace_of_darkness";
    public const string HintTileDesertBonkTorchRoom = "telepathic_tile_desert_bonk_torch_room";
    public const string HintTileCastleTower = "telepathic_tile_castle_tower";
    public const string HintTileIceLargeRoom = "telepathic_tile_ice_large_room";
    public const string HintTileTurtleRock = "telepathic_tile_turtle_rock";
    public const string HintTileIceEntrance = "telepathic_tile_ice_entrance";
    public const string HintTileIceStalfosKnightsRoom = "telepathic_tile_ice_stalfos_knights_room";
    public const string HintTileTowerOfHeraEntrance = "telepathic_tile_tower_of_hera_entrance";
    public const string HintTileSouthEastDarkworldCave = "telepathic_tile_south_east_darkworld_cave";
    public const string GanonsTowerGoalSign = "sign_ganons_tower";
    public const string GanonGoalSign = "sign_ganon";

    private static readonly List<string> s_unwantedText = new()
    {
        "bottle_vendor_choice",
        "bottle_vendor_get",
        "bottle_vendor_no",
        "zora_meeting",
        "zora_tells_cost",
        "zora_get_flippers",
        "zora_no_cash",
        "zora_no_buy_item",
        "fairy_fountain_refill",
        "pond_will_upgrade",
        "pond_item_test",
        "pond_item_bottle_filled"
    };

    internal static readonly IList<(string name, byte[] bytes)> template;

    readonly IList<(string name, byte[] bytes)> entries;

    public StringTable() {
        entries = new List<(string, byte[])>(template);
        foreach (var toRemove in s_unwantedText)
        {
            SetText(toRemove, "{NOTEXT}");
        }
    }

    public StringTable(List<byte[]> previousBytes) {
        entries = new List<(string, byte[])>(template);
        for (var i = 0; i < previousBytes.Count; i++)
        {
            if (i < entries.Count)
            {
                entries[i] = (entries[i].name, previousBytes[i]);
            }
            else
            {
                entries.Add((i.ToString(), previousBytes[i]));
            }
        }
        foreach (var toRemove in s_unwantedText)
        {
            SetText(toRemove, "{NOTEXT}");
        }
    }

    static StringTable() {
        template = ParseEntries("Text.Scripts.StringTable.yaml");
    }

    public void SetHintText(string location, string hint)
    {
        SetText(location, "{NOBORDER}\n" + hint);
    }

    public void SetText(string name, string text) {
        var index = entries.IndexOf(entries.First(x => x.name == name));
        if (index < 0) throw new InvalidOperationException("Invalid text key");
        entries[index] = (name, Dialog.Compiled(text));
    }

    public byte[] GetPaddedBytes() {
        return GetBytes(true);
    }

    public string[] GetLines()
    {
        return entries.Select(x => $"{x.name}: {GetBytePrintString(x.bytes)}").ToArray();
    }

    public string GetBytePrintString(byte[] bytes)
    {
        return string.Join(" ", bytes.Select(x => x.ToString("X2")));
    }

    public byte[] GetBytes(bool pad = false) {
        const int maxBytes = 0x7355;
        var data = entries.SelectMany(x => x.bytes).ToList();

        if (data.Count > maxBytes)
            throw new InvalidOperationException($"String Table exceeds 0x{maxBytes:X} bytes");

        if (pad && data.Count < maxBytes)
            return data.Concat(Enumerable.Repeat<byte>(0xFF, maxBytes - data.Count)).ToArray();
        return data.ToArray();
    }

    static IList<(string, byte[])> ParseEntries(string resource) {
        using var stream = EmbeddedStream.For(resource);
        using var reader = new StreamReader(stream);
        var content = YamlStream.Load(reader).First().Contents as YamlSequence;
        if (content == null)
            throw new InvalidOperationException("Unable to parse string table");

        var convert = new ByteConverter();

        // For each entry in the YAML file, convert strings to dialog entries and
        // arrays into byte arrays
        return content.OfType<YamlMapping>()
            .Select(entry => entry.First())
            .Select(firstEntry => (((YamlValue)firstEntry.Key).Value, firstEntry.Value switch
            {
                YamlSequence bytes => bytes.OfType<YamlValue>()
                    .Select(b => (byte?)convert.ConvertFromInvariantString(b.Value))
                    .OfType<byte>()
                    .ToArray(),
                YamlValue text => Dialog.Compiled(text.Value),
                var o => throw new InvalidOperationException($"Did not expect an object of type {o?.GetType()}"),
            }))
            .ToList();
    }

}
