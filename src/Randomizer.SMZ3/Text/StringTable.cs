using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Randomizer.SMZ3.Generation;
using SharpYaml.Model;

namespace Randomizer.SMZ3.Text;

public class StringTable
{

    public static string SahasrahlaReveal = "sahasrahla_quest_information";
    public static string BombShopReveal = "bomb_shop";
    public static string BlindIntro = "blind_by_the_light";
    public static string TavernMan = "kakariko_tavern_fisherman";
    public static string GanonIntro = "ganon_fall_in";
    public static string GanonPhaseThreeText = "ganon_phase_3";
    public static string TriforceRoom = "end_triforce";
    public static string BottleMerchant = "bottle_vendor_choice";
    public static string KingZora = "zora_tells_cost";
    public static string MasterSwordPedestal = "mastersword_pedestal_translated";
    public static string EtherTablet = "tablet_ether_book";
    public static string BombosTablet = "tablet_bombos_book";
    public static string TelepathicTileEasternPalace = "telepathic_tile_eastern_palace";
    public static string TelepathicTileTowerOfHeraFloor4 = "telepathic_tile_tower_of_hera_floor_4";
    public static string TelepathicTileSpectacleRock = "telepathic_tile_spectacle_rock";
    public static string TelepathicTileSwampEntrance = "telepathic_tile_swamp_entrance";
    public static string TelepathicTileThievesTownUpstairs = "telepathic_tile_thieves_town_upstairs";
    public static string TelepathicTileMiseryMire = "telepathic_tile_misery_mire";
    public static string TelepathicTilePalaceOfDarkness = "telepathic_tile_palace_of_darkness";
    public static string TelepathicTileDesertBonkTorchRoom = "telepathic_tile_desert_bonk_torch_room";
    public static string TelepathicTileCastleTower = "telepathic_tile_castle_tower";
    public static string TelepathicTileIceLargeRoom = "telepathic_tile_ice_large_room";
    public static string TelepathicTileTurtleRock = "telepathic_tile_turtle_rock";
    public static string TelepathicTileIceEntrance = "telepathic_tile_ice_entrance";
    public static string TelepathicTileIceStalfosKnightsRoom = "telepathic_tile_ice_stalfos_knights_room";
    public static string TelepathicTileTowerOfHeraEntrance = "telepathic_tile_tower_of_hera_entrance";
    public static string TelepathicTileSouthEastDarkworldCave = "telepathic_tile_south_east_darkworld_cave";

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

    static StringTable() {
        template = ParseEntries("Text.Scripts.StringTable.yaml");
    }


    public void SetSahasrahlaRevealText(string text) {
        SetText("sahasrahla_quest_information", text);
    }

    public void SetBombShopRevealText(string text) {
        SetText("bomb_shop", text);
    }

    public void SetBlindText(string text) {
        SetText("blind_by_the_light", text);
    }

    public void SetTavernManText(string text) {
        SetText("kakariko_tavern_fisherman", text);
    }

    public void SetGanonFirstPhaseText(string text) {
        SetText("ganon_fall_in", text);
    }

    public void SetGanonThirdPhaseText(string text) {
        SetText("ganon_phase_3", text);
    }

    public void SetTriforceRoomText(string text) {
        SetText("end_triforce", $"{{NOBORDER}}\n{text}");
    }

    public void SetPedestalText(string text) {
        SetText("mastersword_pedestal_translated", text);
    }

    public void SetEtherText(string text) {
        SetText("tablet_ether_book", text);
    }

    public void SetBombosText(string text) {
        SetText("tablet_bombos_book", text);
    }

    public void SetHints(IEnumerable<string> hints)
    {
        var index = 0;
        foreach (var hint in hints)
        {
            SetText(GameHintService.HintLocations[index], "{NOBORDER}\n" + hint);
            index++;
        }
    }

    public void SetHintText(string location, string hint)
    {
        SetText(location, "{NOBORDER}\n" + hint);
    }

    public void SetBottleVendorText(string text)
    {
        SetText("bottle_vendor_choice", text);
    }

    public void SetZoraText(string text)
    {
        SetText("zora_tells_cost", text);
    }

    public void SetText(string name, string text) {
        var index = entries.IndexOf(entries.First(x => x.name == name));
        entries[index] = (name, Dialog.Compiled(text));
    }

    public byte[] GetPaddedBytes() {
        return GetBytes(true);
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
