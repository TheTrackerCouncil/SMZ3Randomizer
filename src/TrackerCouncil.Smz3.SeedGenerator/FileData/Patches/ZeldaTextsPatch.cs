using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.Text;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[Order(-5)]
public class ZeldaTextsPatch : RomPatch
{
    private StringTable _stringTable = null!;
    private PlandoTextConfig _plandoText = null!;
    private readonly GameLinesConfig _gameLines;
    private readonly ItemConfig _items;
    private readonly RegionConfig _regions;
    private IGameHintService _gameHintService;
    private GetPatchesRequest _data = null!;

    public ZeldaTextsPatch(Configs configs, IGameHintService gameHintService)
    {
        _gameHintService = gameHintService;
        _gameLines = configs.GameLines;
        _items = configs.Items;
        _regions = configs.Regions;
    }

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        _data = data;
        _stringTable = new StringTable();
        _plandoText = data.PlandoConfig.Text;

        var regions = data.World.Regions.OfType<IHasReward>().ToList();

        var greenPendantDungeon = regions
            .Where(x => x.RewardType == RewardType.PendantGreen)
            .Select(x => GetRegionName((Region)x))
            .First();

        var redCrystalDungeons = regions
            .Where(x => x.RewardType == RewardType.CrystalRed)
            .Select(x => GetRegionName((Region)x))
            .ToList();

        yield return SetText(0x308A00, StringTable.SahasrahlaReveal,
            _gameLines.SahasrahlaReveal, _plandoText.SahasrahlaReveal, false,
            greenPendantDungeon);

        yield return SetText(0x308E00, StringTable.BombShopReveal,
            _gameLines.BombShopReveal, _plandoText.BombShopReveal, false,
            redCrystalDungeons.First(), redCrystalDungeons.Last());

        yield return SetText(0x308800, StringTable.BlindIntro,
            _gameLines.BlindIntro, _plandoText.BlindIntro);

        yield return SetText(0x308C00, StringTable.TavernMan,
            _gameLines.TavernMan, _plandoText.TavernMan);

        foreach (var text in GanonText(data))
        {
            yield return text;
        }

        SetMerchantText();

        var hintText = GetPedestalHint(data, LocationId.MasterSwordPedestal);
        yield return SetText(0x308300, StringTable.MasterSwordPedestal, hintText, _plandoText.MasterSwordPedestal);

        hintText = GetPedestalHint(data, LocationId.EtherTablet);
        yield return SetText(0x308F00, StringTable.EtherTablet, hintText, _plandoText.EtherTablet);

        hintText = GetPedestalHint(data, LocationId.BombosTablet);
        yield return SetText(0x309000, StringTable.BombosTablet, hintText, _plandoText.BombosTablet);

        yield return SetText(0x308400, StringTable.TriforceRoom,
            _gameLines.TriforceRoom, _plandoText.TriforceRoom, true);

        if (data.World.HintTiles.Any() || _plandoText.HasHintTileText)
        {
            SetHintText();
        }


        yield return new GeneratedPatch(Snes(0x1C8000), _stringTable.GetPaddedBytes());
    }

    private GeneratedPatch SetText(int address, string? textKey, SchrodingersString? defaultText, string? overrideText, bool hideBorder = false, params object[] formatData)
    {
        return SetText(address, textKey, defaultText?.ToString(), overrideText, hideBorder, formatData);
    }

    private GeneratedPatch SetText(int address, string? textKey, string? defaultText, string? overrideText, bool hideBorder = false, params object[] formatData)
    {
        var text = string.IsNullOrEmpty(overrideText) ? defaultText : overrideText;
        if (string.IsNullOrEmpty(text))
            text = "{NOTEXT}";

        var formattedText =
            Dialog.GetGameSafeString(string.Format(text, formatData));

        if (!string.IsNullOrEmpty(textKey))
        {
            var stringFlag = hideBorder ? "{NOBORDER}\n" : "";
            _stringTable.SetText(textKey, $"{stringFlag}{formattedText}");
        }

        if (address < 0)
        {
            return new GeneratedPatch(0, Array.Empty<byte>());
        }

        return new GeneratedPatch(Snes(address), Dialog.Simple(formattedText));
    }

    private void SetChoiceText(string textKey, SchrodingersString? defaultText, string? overrideText, string item)
    {
        var text = string.IsNullOrEmpty(overrideText) ? defaultText?.ToString() : overrideText;
        if (string.IsNullOrEmpty(text))
            text = "{NOTEXT}";

        _stringTable.SetText(textKey,
            Dialog.GetChoiceText(string.Format(text, item),
                _gameLines.ChoiceYes?.ToString() ?? "Yes",
                _gameLines.ChoiceNo?.ToString() ?? "No"));

    }

    private IEnumerable<GeneratedPatch> GanonText(GetPatchesRequest data)
    {
        yield return SetText(0x308600, StringTable.GanonIntro,
            _gameLines.GanonIntro, _plandoText.GanonIntro);

        // Todo: Verify these two are correct if ganon invincible patch is
        // ever added ganon_fall_in_alt in v30
        var ganonFirstPhaseInvincible = "You think you\nare ready to\nface me?\n\nI will not die\n\nunless you\ncomplete your\ngoals. Dingus!";
        yield return new GeneratedPatch(Snes(0x309100), Dialog.Simple(ganonFirstPhaseInvincible));

        // ganon_phase_3_alt in v30
        var ganonThirdPhaseInvincible = "Got wax in\nyour ears?\nI cannot die!";
        yield return new GeneratedPatch(Snes(0x309200), Dialog.Simple(ganonThirdPhaseInvincible));
        // ---

        var silversLocation = data.Worlds.SelectMany(world => world.Locations)
            .FirstOrDefault(l => l.ItemIs(ItemType.SilverArrows, data.World));
        var silversText = silversLocation == null
            ? _gameLines.GanonNoSilvers
            : _gameLines.GanonSilversHint;
        var silverLocationPlayer = data.Config.MultiWorld && silversLocation?.World != data.World
            ? silversLocation?.World.Player
            : "you";
        yield return SetText(0x308700, StringTable.GanonPhaseThreeText,
            silversText, _plandoText.GanonSilversHint, false,
            silverLocationPlayer ?? "", silversLocation?.Region.Area ?? "");
    }

    private void SetMerchantText()
    {
        // Have bottle merchant and zora say what they have if requested
        if (_data.Config.CasPatches.PreventScams)
        {
            var item = GetItemName(_data, _data.World.LightWorldNorthWest.BottleMerchant.Item);
            SetChoiceText(StringTable.BottleMerchant, _gameLines.BottleMerchant,
                _plandoText.BottleMerchant, item);

            item = GetItemName(_data, _data.World.FindLocation(LocationId.KingZora).Item);
            SetChoiceText(StringTable.KingZora, _gameLines.KingZora,
                _plandoText.KingZora, item);
        }
        else
        {
            if (!string.IsNullOrEmpty(_plandoText.BottleMerchant))
            {
                SetChoiceText(StringTable.BottleMerchant, null, _plandoText.BottleMerchant, "");
            }

            if (!string.IsNullOrEmpty(_plandoText.KingZora))
            {
                SetChoiceText(StringTable.KingZora, null, _plandoText.KingZora, "");
            }
        }
    }

    private void SetHintText()
    {
        var hints = _data.World.HintTiles.ToDictionary(x => x.HintTileCode, x => x);

        SetHintTileText(StringTable.HintTileEasternPalace, hints,
            _plandoText.HintTileEasternPalace);
        SetHintTileText(StringTable.HintTileTowerOfHeraFloor4, hints,
            _plandoText.HintTileTowerOfHeraFloor4);
        SetHintTileText(StringTable.HintTileSpectacleRock, hints,
            _plandoText.HintTileSpectacleRock);
        SetHintTileText(StringTable.HintTileSwampEntrance, hints,
            _plandoText.HintTileSwampEntrance);
        SetHintTileText(StringTable.HintTileThievesTownUpstairs, hints,
            _plandoText.HintTileThievesTownUpstairs);
        SetHintTileText(StringTable.HintTileMiseryMire, hints,
            _plandoText.HintTileMiseryMire);
        SetHintTileText(StringTable.HintTilePalaceOfDarkness, hints,
            _plandoText.HintTilePalaceOfDarkness);
        SetHintTileText(StringTable.HintTileDesertBonkTorchRoom, hints,
            _plandoText.HintTileDesertBonkTorchRoom);
        SetHintTileText(StringTable.HintTileCastleTower, hints,
            _plandoText.HintTileCastleTower);
        SetHintTileText(StringTable.HintTileIceLargeRoom, hints,
            _plandoText.HintTileIceLargeRoom);
        SetHintTileText(StringTable.HintTileTurtleRock, hints,
            _plandoText.HintTileTurtleRock);
        SetHintTileText(StringTable.HintTileIceEntrance, hints,
            _plandoText.HintTileIceEntrance);
        SetHintTileText(StringTable.HintTileIceStalfosKnightsRoom, hints,
            _plandoText.HintTileIceStalfosKnightsRoom);
        SetHintTileText(StringTable.HintTileTowerOfHeraEntrance, hints,
            _plandoText.HintTileTowerOfHeraEntrance);
        SetHintTileText(StringTable.HintTileSouthEastDarkworldCave, hints,
            _plandoText.HintTileSouthEastDarkworldCave);
    }

    private void SetHintTileText(string key, Dictionary<string, PlayerHintTile> hints, string? overrideText)
    {
        if (!string.IsNullOrEmpty(overrideText))
        {
            _stringTable.SetHintText(key, Dialog.GetGameSafeString(overrideText));
        }
        else if (hints.TryGetValue(key, out var hint))
        {
            var hintText = _gameHintService.GetHintTileText(hint, _data.World, _data.Worlds);
            if (!string.IsNullOrEmpty(hintText))
            {
                _stringTable.SetHintText(key, Dialog.GetGameSafeString(hintText));
            }
        }
    }

    private string GetRegionName(Region region)
    {
        return Dialog.GetGameSafeString(GetRegionInfo(region)?.Name?.ToString() ?? region.Name);
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

    private string GetPedestalHint(GetPatchesRequest data, LocationId locationId)
    {
        var item = data.World.FindLocation(locationId).Item;
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
