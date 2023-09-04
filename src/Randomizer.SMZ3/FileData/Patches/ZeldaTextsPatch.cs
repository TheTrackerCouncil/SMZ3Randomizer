using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Text;

namespace Randomizer.SMZ3.FileData.Patches;

[Order(-5)]
public class ZeldaTextsPatch : RomPatch
{
    private StringTable _stringTable = null!;
    private readonly GameLinesConfig _gameLines;
    private readonly ItemConfig _items;
    private readonly RegionConfig _regions;

    public ZeldaTextsPatch(Configs configs)
    {
        _gameLines = configs.GameLines;
        _items = configs.Items;
        _regions = configs.Regions;
    }

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        _stringTable = new StringTable();

        var regions = data.World.Regions.OfType<IHasReward>();

        var greenPendantDungeon = regions
            .Where(x => x.RewardType == RewardType.PendantGreen)
            .Select(x => GetRegionName((Region)x))
            .First();

        var redCrystalDungeons = regions
            .Where(x => x.RewardType == RewardType.CrystalRed)
            .Select(x => GetRegionName((Region)x));

        var sahasrahla =
            Dialog.GetGameSafeString(_gameLines.SahasrahlaReveal?.Format(greenPendantDungeon) ?? "{NOTEXT}");
        yield return new GeneratedPatch(Snes(0x308A00), Dialog.Simple(sahasrahla));
        _stringTable.SetSahasrahlaRevealText(sahasrahla);

        var bombShop =
            Dialog.GetGameSafeString(
                _gameLines.BombShopReveal?.Format(redCrystalDungeons.First(), redCrystalDungeons.Last()) ??
                "{NOTEXT}");
        yield return new GeneratedPatch(Snes(0x308E00), Dialog.Simple(bombShop));
        _stringTable.SetBombShopRevealText(bombShop);

        var blind = Dialog.GetGameSafeString(_gameLines.BlindIntro?.ToString() ?? "{NOTEXT}");
        yield return new GeneratedPatch(Snes(0x308800), Dialog.Simple(blind));
        _stringTable.SetBlindText(blind);

        var tavernMan = Dialog.GetGameSafeString(_gameLines.TavernMan?.ToString() ?? "{NOTEXT}");
        yield return new GeneratedPatch(Snes(0x308C00), Dialog.Simple(tavernMan));
        _stringTable.SetTavernManText(tavernMan);

        var ganon = Dialog.GetGameSafeString(_gameLines.GanonIntro?.ToString() ?? "{NOTEXT}");
        yield return new GeneratedPatch(Snes(0x308600), Dialog.Simple(ganon));
        _stringTable.SetGanonFirstPhaseText(ganon);

        // Have bottle merchant and zora say what they have if requested
        if (data.Config.CasPatches.PreventScams)
        {
            var item = GetItemName(data, data.World.LightWorldNorthWest.BottleMerchant.Item);
            _stringTable.SetBottleVendorText(Dialog.GetChoiceText(_gameLines.BottleMerchant?.Format(item) ?? "{NOTEXT}", _gameLines.ChoiceYes?.ToString() ?? string.Empty, _gameLines.ChoiceNo?.ToString() ?? string.Empty));

            item = GetItemName(data, data.World.FindLocation(LocationId.KingZora).Item);
            _stringTable.SetZoraText(Dialog.GetChoiceText(_gameLines.KingZora?.Format(item) ?? "{NOTEXT}", _gameLines.ChoiceYes?.ToString() ?? string.Empty, _gameLines.ChoiceNo?.ToString() ?? string.Empty));
        }

        // Todo: Verify these two are correct if ganon invincible patch is
        // ever added ganon_fall_in_alt in v30
        var ganonFirstPhaseInvincible = "You think you\nare ready to\nface me?\n\nI will not die\n\nunless you\ncomplete your\ngoals. Dingus!";
        yield return new GeneratedPatch(Snes(0x309100), Dialog.Simple(ganonFirstPhaseInvincible));

        // ganon_phase_3_alt in v30
        var ganonThirdPhaseInvincible = "Got wax in\nyour ears?\nI cannot die!";
        yield return new GeneratedPatch(Snes(0x309200), Dialog.Simple(ganonThirdPhaseInvincible));
        // ---

        var silversLocation = data.Worlds.SelectMany(world => world.Locations).FirstOrDefault(l => l.ItemIs(ItemType.SilverArrows, data.World));
        if (silversLocation != null)
        {
            var silvers = Dialog.GetGameSafeString(_gameLines.GanonSilversHint?.Format(
                !data.Config.MultiWorld || silversLocation.World == data.World ? "you" : silversLocation.World.Player,
                silversLocation.Region.Area) ?? "{NOTEXT}");
            yield return new GeneratedPatch(Snes(0x308700), Dialog.Simple(silvers));
            _stringTable.SetGanonThirdPhaseText(silvers);
        }
        else
        {
            var silvers = Dialog.GetGameSafeString(_gameLines.GanonNoSilvers?.ToString() ?? "{NOTEXT}");
            yield return new GeneratedPatch(Snes(0x308700), Dialog.Simple(silvers));
            _stringTable.SetGanonThirdPhaseText(silvers);
        }

        yield return PedestalTabletText(data, data.World.FindLocation(LocationId.MasterSwordPedestal));
        yield return PedestalTabletText(data, data.World.FindLocation(LocationId.EtherTablet));
        yield return PedestalTabletText(data, data.World.FindLocation(LocationId.BombosTablet));

        var triforceRoom = Dialog.GetGameSafeString(_gameLines.TriforceRoom?.ToString() ?? "{NOTEXT}");
        yield return new GeneratedPatch(Snes(0x308400), Dialog.Simple(triforceRoom));
        _stringTable.SetTriforceRoomText(triforceRoom);

        if (data.Hints.Any() && data.Config.UniqueHintCount > 0)
        {
            var hints = data.Hints.Take(data.Config.UniqueHintCount);
            while (hints.Count() < GameHintService.HintLocations.Count)
            {
                hints = hints.Concat(hints.Take(Math.Min(GameHintService.HintLocations.Count() - hints.Count(), hints.Count())));
            }
            _stringTable.SetHints(hints.Shuffle(data.Random).Select(Dialog.GetGameSafeString));
        }

        yield return new GeneratedPatch(Snes(0x1C8000), _stringTable.GetPaddedBytes());
    }

    private GeneratedPatch PedestalTabletText(GetPatchesRequest data, Location location)
    {
        var text = GetPedestalHint(data, location.Item);
        var dialog = Dialog.Simple(text);
        if (location.Type == LocationType.Pedestal)
        {
            _stringTable.SetPedestalText(text);
            return new GeneratedPatch(Snes(0x308300), dialog);
        }
        else if (location.Type == LocationType.Ether)
        {
            _stringTable.SetEtherText(text);
            return new GeneratedPatch(Snes(0x308F00), dialog);
        }
        else if (location.Type == LocationType.Bombos)
        {
            _stringTable.SetBombosText(text);
            return new GeneratedPatch(Snes(0x309000), dialog);
        }

        return new GeneratedPatch(-1, Array.Empty<byte>());
    }

    private string GetRegionName(Region region)
    {
        return Dialog.GetGameSafeString(GetRegionInfo(region)?.Name.ToString() ?? region.Name);
    }

    private string GetItemName(GetPatchesRequest data, Item item)
    {
        var itemName = GetItemData(item)?.NameWithArticle ?? item.Name;
        if (!data.Config.MultiWorld)
        {
            return itemName;
        }
        else
        {
            return data.World == item.World
                ? $"{itemName} belonging to you"
                : $"{itemName} belonging to {item.World.Player}";
        }
    }

    private string GetPedestalHint(GetPatchesRequest data, Item item)
    {
        var hintText = GetItemData(item)?.PedestalHints?.ToString() ?? item.Name;
        if (!data.Config.MultiWorld)
        {
            return hintText;
        }
        else
        {
            return data.World == item.World
                ? $"{hintText} belonging to you"
                : $"{hintText} belonging to {item.World.Player}";
        }
    }

    private RegionInfo? GetRegionInfo(Region region) => _regions.FirstOrDefault(x => x.Type == region.GetType());
    private ItemData? GetItemData(Item item) => _items.FirstOrDefault(x => x.InternalItemType == item.Type);
}
