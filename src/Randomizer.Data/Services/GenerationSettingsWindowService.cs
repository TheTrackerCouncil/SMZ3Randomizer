using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using Randomizer.Data.GeneratedData;
using Randomizer.Data.Interfaces;
using Randomizer.Data.Options;
using Randomizer.Data.ViewModels;

namespace Randomizer.Data.Services;

public class GenerationSettingsWindowService(SpriteService spriteService, OptionsFactory optionsFactory, IRomGenerationService _romGenerator, ILogger<GenerationSettingsWindowService> logger)
{
    private RandomizerOptions _options = null!;
    private GenerationWindowViewModel _model = null!;

    public GenerationWindowViewModel GetViewModel()
    {
        _options = optionsFactory.Create();
        _model = new GenerationWindowViewModel(_options);
        UpdateMsuText();
        _ = LoadSprites();
        return _model;
    }

    public void SaveSpriteSettings(bool userAccepted, Sprite sprite, Dictionary<string, SpriteOptions> selectedSpriteOptions, string searchText, SpriteFilter filter)
    {
        if (sprite.SpriteType == SpriteType.Link)
        {
            _options.GeneralOptions.LinkSpriteOptions = selectedSpriteOptions;

            if (userAccepted)
            {
                _options.PatchOptions.SelectedLinkSprite = sprite;
                _options.GeneralOptions.LinkSpriteSearchText = searchText;
                _options.GeneralOptions.LinkSpriteFilter = filter;
            }
        }
        else if (sprite.SpriteType == SpriteType.Samus)
        {
            _options.GeneralOptions.SamusSpriteOptions = selectedSpriteOptions;

            if (userAccepted)
            {
                _options.PatchOptions.SelectedSamusSprite = sprite;
                _options.GeneralOptions.SamusSpriteSearchText = searchText;
                _options.GeneralOptions.SamusSpriteFilter = filter;
            }
        }
        else if (sprite.SpriteType == SpriteType.Ship)
        {
            _options.GeneralOptions.ShipSpriteOptions = selectedSpriteOptions;

            if (userAccepted)
            {
                _options.PatchOptions.SelectedShipSprite = sprite;
                _options.GeneralOptions.ShipSpriteSearchText = searchText;
                _options.GeneralOptions.ShipSpriteFilter = filter;
            }
        }

        _options.Save();

        UpdateSpriteDetails(sprite.SpriteType, sprite);
    }

    public void UpdateMsuText()
    {
        if (_options.PatchOptions.MsuRandomizationStyle == null && _options.PatchOptions.MsuPaths.Any())
        {
            if (!string.IsNullOrEmpty(_options.GeneralOptions.MsuPath))
            {
                _model.Basic.MsuText = Path.GetRelativePath(_options.GeneralOptions.MsuPath, _options.PatchOptions.MsuPaths.First());
            }
            else
            {
                _model.Basic.MsuText = _options.PatchOptions.MsuPaths.First();
            }
        }
        else if (_options.PatchOptions.MsuRandomizationStyle == MsuRandomizationStyle.Single)
        {
            _model.Basic.MsuText = $"Random MSU from {_options.PatchOptions.MsuPaths.Count} MSUs";
        }
        else if (_options.PatchOptions.MsuRandomizationStyle == MsuRandomizationStyle.Shuffled)
        {
            _model.Basic.MsuText = $"Shuffled MSU from {_options.PatchOptions.MsuPaths.Count} MSUs";
        }
        else if (_options.PatchOptions.MsuRandomizationStyle == MsuRandomizationStyle.Continuous)
        {
            _model.Basic.MsuText = $"Continuously Shuffle {_options.PatchOptions.MsuPaths.Count} MSUs";
        }
        else
        {
            _model.Basic.MsuText = "None";
        }
    }

    public void SetMsuPath(string path)
    {
        _options.PatchOptions.MsuPaths = new List<string>() { path };
        _options.PatchOptions.MsuRandomizationStyle = null;
        UpdateMsuText();
    }

    public void ClearMsu()
    {
        _options.PatchOptions.MsuPaths = new List<string>();
        _options.PatchOptions.MsuRandomizationStyle = null;
        UpdateMsuText();
    }

    public void SaveSettings()
    {

    }

    public async Task<GeneratedRomResult> GenerateRom()
    {
        return _model.IsPlando
            ? await _romGenerator.GeneratePlandoRomAsync(_options, _model.PlandoConfig!)
            : await _romGenerator.GenerateRandomRomAsync(_options);
    }

    private async Task LoadSprites()
    {
        try
        {
            await spriteService.LoadSpritesAsync();
            UpdateSprites();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading sprites");
        }
    }

    private void UpdateSprites()
    {
        UpdateSpriteDetails(SpriteType.Link);
        UpdateSpriteDetails(SpriteType.Samus);
        UpdateSpriteDetails(SpriteType.Ship);
    }

    private void UpdateSpriteDetails(SpriteType type, Sprite? sprite = null)
    {
        if (sprite == null)
        {
            if (type == SpriteType.Link)
            {
                sprite = _options.PatchOptions.SelectedLinkSprite;
            }
            else if (type == SpriteType.Samus)
            {
                sprite = _options.PatchOptions.SelectedSamusSprite;
            }
            else
            {
                sprite = _options.PatchOptions.SelectedShipSprite;
            }

            if (sprite is { IsDefault: false, IsRandomSprite: false })
            {
                sprite = spriteService.GetSprite(type);
            }
        }

        string spriteName;
        string spritePath;

        if (sprite.IsDefault)
        {
            sprite = type switch
            {
                SpriteType.Link => Sprite.DefaultLink,
                SpriteType.Samus => Sprite.DefaultSamus,
                _ => Sprite.DefaultShip
            };
            spriteName = sprite.Name;
            spritePath = sprite.PreviewPath;
        }
        else if (sprite.IsRandomSprite)
        {
            sprite = type switch
            {
                SpriteType.Link => Sprite.RandomLink,
                SpriteType.Samus => Sprite.RandomSamus,
                _ => Sprite.RandomShip
            };
            spriteName = sprite.Name;
            spritePath = Sprite.RandomSamus.PreviewPath;
        }
        else
        {
            spriteName = sprite.ToString();
            spritePath = sprite.PreviewPath;
        }

        if (type == SpriteType.Link)
        {
            _model.Basic.LinkSpriteName = spriteName;
            _model.Basic.LinkSpritePath = spritePath;
        }
        else if (type == SpriteType.Samus)
        {
            _model.Basic.SamusSpriteName = spriteName;
            _model.Basic.SamusSpritePath = spritePath;
        }
        else if (type == SpriteType.Ship)
        {
            _model.Basic.ShipSpriteName = spriteName;
            _model.Basic.ShipSpritePath = spritePath;
        }
    }


}
