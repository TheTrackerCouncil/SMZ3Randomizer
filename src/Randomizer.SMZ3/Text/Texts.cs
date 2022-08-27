using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Randomizer.Shared;
using Randomizer.SMZ3.Regions.Zelda;

using SharpYaml.Model;

namespace Randomizer.SMZ3.Text
{
    public static class Texts
    {
        private static readonly YamlMapping scripts;
        private static readonly IList<string> blind;
        private static readonly IList<string> ganon;
        private static readonly IList<string> tavernMan;
        private static readonly IList<string> triforceRoom;

        static Texts()
        {
            scripts = ParseYamlScripts("Text.Scripts.General.yaml") as YamlMapping;
            blind = ParseTextScript("Text.Scripts.BlindExtra.txt");
            ganon = ParseTextScript("Text.Scripts.GanonExtra.txt");
            tavernMan = ParseTextScript("Text.Scripts.TavernMan.txt");
            triforceRoom = ParseTextScript("Text.Scripts.TriforceRoomExtra.txt");
        }

        private static YamlElement ParseYamlScripts(string resource)
        {
            using var stream = EmbeddedStream.For(resource);
            using var reader = new StreamReader(stream);
            return YamlStream.Load(reader).First().Contents;
        }

        private static IList<string> ParseTextScript(string resource)
        {
            using var stream = EmbeddedStream.For(resource);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd().Replace("\r", "").Split("---\n")
                .Select(text => text.TrimEnd('\n'))
                .Where(text => text != string.Empty)
                .ToList();
        }

        public static string SahasrahlaReveal(Region dungeon)
        {
            var text = (scripts["SahasrahlaReveal"] as YamlValue).Value;
            return text.Replace("<dungeon>", dungeon.Area);
        }

        public static string BombShopReveal(IEnumerable<Region> dungeons)
        {
            var (first, second, _) = dungeons;
            var text = (scripts["BombShopReveal"] as YamlValue).Value;
            return text.Replace("<first>", first.Area).Replace("<second>", second.Area);
        }

        public static string GanonThirdPhraseNone()
        {
            var text = ((scripts["GanonSilversReveal"] as YamlMapping)["none"] as YamlValue).Value;
            return text;
        }

        public static string GanonThirdPhaseSingle(Region silvers)
        {
            var node = (scripts["GanonSilversReveal"] as YamlMapping)["single"] as YamlMapping;
            var text = (node[silvers is GanonsTower ? "local" : "remote"] as YamlValue).Value;
            return text.Replace("<region>", silvers.Area);
        }

        public static string GanonThirdPhaseMulti(Region silvers, World myWorld)
        {
            var node = (scripts["GanonSilversReveal"] as YamlMapping)["multi"] as YamlMapping;
            if (silvers.World == myWorld)
                return (node["local"] as YamlValue).Value;
            var player = silvers.World.Player;
            player = player.PadLeft(7 + player.Length / 2);
            var text = (node["remote"] as YamlValue).Value;
            return text.Replace("<player>", player);
        }

        public static string ItemTextbox(Item item)
        {
            var name = item.Type switch
            {
                _ when item.IsMap => "Map",
                _ when item.IsCompass => "Compass",
                _ when item.IsKeycard => "Keycard",
                ItemType.BottleWithGoldBee => ItemType.BottleWithBee.ToString(),
                ItemType.HeartContainerRefill => ItemType.HeartContainer.ToString(),
                ItemType.OneRupee => "PocketRupees",
                ItemType.FiveRupees => "PocketRupees",
                ItemType.TwentyRupees => "CouchRupees",
                ItemType.TwentyRupees2 => "CouchRupees",
                ItemType.FiftyRupees => "CouchRupees",
                ItemType.BombUpgrade5 => "BombUpgrade",
                ItemType.BombUpgrade10 => "BombUpgrade",
                ItemType.ArrowUpgrade5 => "ArrowUpgrade",
                ItemType.ArrowUpgrade10 => "ArrowUpgrade",
                var type => type.ToString(),
            };

            var items = scripts["Items"] as YamlMapping;
            return (items[name] as YamlValue)?.Value ?? (items["default"] as YamlValue).Value;
        }

        public static string Blind(Random rnd) => RandomLine(rnd, blind);

        public static string TavernMan(Random rnd) => RandomLine(rnd, tavernMan);

        public static string GanonFirstPhase(Random rnd) => RandomLine(rnd, ganon);

        public static string TriforceRoom(Random rnd) => RandomLine(rnd, triforceRoom);

        private static string RandomLine(Random rnd, IList<string> lines) => lines[rnd.Next(lines.Count)];

    }

}
