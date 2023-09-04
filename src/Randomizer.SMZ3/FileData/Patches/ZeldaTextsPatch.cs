using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Text;

namespace Randomizer.SMZ3.FileData.Patches;

public class ZeldaTextsPatch : RomPatch
{
    private PatcherServiceData _data = null!;
    private StringTable _stringTable = null!;

    public override IEnumerable<(int offset, byte[] data)> GetChanges(PatcherServiceData data)
    {
        _data = data;
        _stringTable = new StringTable();

        var regions = data.LocalWorld.Regions.OfType<IHasReward>();

        var greenPendantDungeon = regions
            .Where(x => x.RewardType == RewardType.PendantGreen)
            .Select(x => GetRegionName((Region)x))
            .First();

        var redCrystalDungeons = regions
            .Where(x => x.RewardType == RewardType.CrystalRed)
            .Select(x => GetRegionName((Region)x));

        var sahasrahla =
            Dialog.GetGameSafeString(data.GameLines.SahasrahlaReveal?.Format(greenPendantDungeon) ?? "{NOTEXT}");
        yield return (Snes(0x308A00), Dialog.Simple(sahasrahla));
        _stringTable.SetSahasrahlaRevealText(sahasrahla);

        var bombShop =
            Dialog.GetGameSafeString(
                data.GameLines.BombShopReveal?.Format(redCrystalDungeons.First(), redCrystalDungeons.Last()) ??
                "{NOTEXT}");
        yield return (Snes(0x308E00), Dialog.Simple(bombShop));
        _stringTable.SetBombShopRevealText(bombShop);

        var blind = Dialog.GetGameSafeString(data.GameLines.BlindIntro?.ToString() ?? "{NOTEXT}");
        yield return (Snes(0x308800), Dialog.Simple(blind));
        _stringTable.SetBlindText(blind);

        var tavernMan = Dialog.GetGameSafeString(data.GameLines.TavernMan?.ToString() ?? "{NOTEXT}");
        yield return (Snes(0x308C00), Dialog.Simple(tavernMan));
        _stringTable.SetTavernManText(tavernMan);

        var ganon = Dialog.GetGameSafeString(data.GameLines.GanonIntro?.ToString() ?? "{NOTEXT}");
        yield return (Snes(0x308600), Dialog.Simple(ganon));
        _stringTable.SetGanonFirstPhaseText(ganon);

        // Have bottle merchant and zora say what they have if requested
        if (data.Config.CasPatches.PreventScams)
        {
            var item = GetItemName(data, data.LocalWorld.LightWorldNorthWest.BottleMerchant.Item);
            _stringTable.SetBottleVendorText(Dialog.GetChoiceText(data.GameLines.BottleMerchant?.Format(item) ?? "{NOTEXT}", data.GameLines.ChoiceYes?.ToString() ?? string.Empty, data.GameLines.ChoiceNo?.ToString() ?? string.Empty));

            item = GetItemName(data, data.LocalWorld.FindLocation(LocationId.KingZora).Item);
            _stringTable.SetZoraText(Dialog.GetChoiceText(data.GameLines.KingZora?.Format(item) ?? "{NOTEXT}", data.GameLines.ChoiceYes?.ToString() ?? string.Empty, data.GameLines.ChoiceNo?.ToString() ?? string.Empty));
        }

        // Todo: Verify these two are correct if ganon invincible patch is
        // ever added ganon_fall_in_alt in v30
        var ganonFirstPhaseInvincible = "You think you\nare ready to\nface me?\n\nI will not die\n\nunless you\ncomplete your\ngoals. Dingus!";
        yield return (Snes(0x309100), Dialog.Simple(ganonFirstPhaseInvincible));

        // ganon_phase_3_alt in v30
        var ganonThirdPhaseInvincible = "Got wax in\nyour ears?\nI cannot die!";
        yield return (Snes(0x309200), Dialog.Simple(ganonThirdPhaseInvincible));
        // ---

        var silversLocation = data.Worlds.SelectMany(world => world.Locations).FirstOrDefault(l => l.ItemIs(ItemType.SilverArrows, data.LocalWorld));
        if (silversLocation != null)
        {
            var silvers = Dialog.GetGameSafeString(data.GameLines.GanonSilversHint?.Format(
                !data.Config.MultiWorld || silversLocation.World == data.LocalWorld ? "you" : silversLocation.World.Player,
                silversLocation.Region.Area) ?? "{NOTEXT}");
            yield return (Snes(0x308700), Dialog.Simple(silvers));
            _stringTable.SetGanonThirdPhaseText(silvers);
        }
        else
        {
            var silvers = Dialog.GetGameSafeString(data.GameLines.GanonNoSilvers?.ToString() ?? "{NOTEXT}");
            yield return (Snes(0x308700), Dialog.Simple(silvers));
            _stringTable.SetGanonThirdPhaseText(silvers);
        }

        yield return PedestalTabletText(data, data.LocalWorld.FindLocation(LocationId.MasterSwordPedestal));
        yield return PedestalTabletText(data, data.LocalWorld.FindLocation(LocationId.EtherTablet));
        yield return PedestalTabletText(data, data.LocalWorld.FindLocation(LocationId.BombosTablet));

        var triforceRoom = Dialog.GetGameSafeString(data.GameLines.TriforceRoom?.ToString() ?? "{NOTEXT}");
        yield return (Snes(0x308400), Dialog.Simple(triforceRoom));
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

        yield return (Snes(0x1C8000), _stringTable.GetPaddedBytes());
    }

    private (int offset, byte[] data) PedestalTabletText(PatcherServiceData data, Location location)
    {
        var text = GetPedestalHint(data, location.Item);
        var dialog = Dialog.Simple(text);
        if (location.Type == LocationType.Pedestal)
        {
            _stringTable.SetPedestalText(text);
            return (Snes(0x308300), dialog);
        }
        else if (location.Type == LocationType.Ether)
        {
            _stringTable.SetEtherText(text);
            return (Snes(0x308F00), dialog);
        }
        else if (location.Type == LocationType.Bombos)
        {
            _stringTable.SetBombosText(text);
            return (Snes(0x309000), dialog);
        }

        return (-1, Array.Empty<byte>());
    }

    private string GetRegionName(Region region)
    {
        return Dialog.GetGameSafeString(_data.GetRegionInfo(region)?.Name.ToString() ?? region.Name);
    }

    private string GetItemName(PatcherServiceData data, Item item)
    {
        var itemName = _data.GetItemData(item)?.NameWithArticle ?? item.Name;
        if (!data.Config.MultiWorld)
        {
            return itemName;
        }
        else
        {
            return data.LocalWorld == item.World
                ? $"{itemName} belonging to you"
                : $"{itemName} belonging to {item.World.Player}";
        }
    }

    private string GetPedestalHint(PatcherServiceData data, Item item)
    {
        var hintText = data.GetItemData(item)?.PedestalHints?.ToString() ?? item.Name;
        if (!data.Config.MultiWorld)
        {
            return hintText;
        }
        else
        {
            return data.LocalWorld == item.World
                ? $"{hintText} belonging to you"
                : $"{hintText} belonging to {item.World.Player}";
        }
    }
}
