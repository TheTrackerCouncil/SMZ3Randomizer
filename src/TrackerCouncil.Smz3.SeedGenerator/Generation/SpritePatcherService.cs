using System.IO;
using System.Linq;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.SeedGenerator.FileData;
using TrackerCouncil.Smz3.SeedGenerator.FileData.IpsPatches;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class SpritePatcherService
{
    private SpriteService _spriteService;

    public SpritePatcherService(SpriteService spriteService)
    {
        _spriteService = spriteService;
    }

    public void ApplySpriteOptions(byte[] bytes, out string linkSpriteName, out string samusSpriteName)
    {
        var linkSprite = _spriteService.Sprites.First(x => x is { SpriteType: SpriteType.Link, Name: "Captain Novolin" });
        ApplyRdcSpriteTo(linkSprite, bytes);
        linkSpriteName = string.IsNullOrEmpty(linkSprite.Name) ? "Link" : linkSprite.Name;

        var samusSprite = _spriteService.Sprites.First(x => x is { SpriteType: SpriteType.Samus, Name: "Captain Novolin" });
        ApplyRdcSpriteTo(samusSprite, bytes);
        samusSpriteName = string.IsNullOrEmpty(samusSprite.Name) ? "Samus" : samusSprite.Name;

        var shipSprite = _spriteService.Sprites.First(x => x is { SpriteType: SpriteType.Ship, Name: "Captain's Endocrinologist" });
        ApplyShipSpriteTo(shipSprite, bytes);
    }

    private void ApplyShipSpriteTo(Sprite sprite, byte[] bytes)
    {
        if (sprite.IsDefault) return;

        var shipPatchFileName = sprite.FilePath;
        if (File.Exists(shipPatchFileName))
        {
            using var customShipBasePatch = IpsPatch.CustomShip();
            Rom.ApplySuperMetroidIps(bytes, customShipBasePatch);

            using var shipPatch = File.OpenRead(shipPatchFileName);
            Rom.ApplySuperMetroidIps(bytes, shipPatch);
        }
    }

    private void ApplyRdcSpriteTo(Sprite sprite, byte[] bytes)
    {
        if (sprite.IsDefault) return;

        using var stream = File.OpenRead(sprite.FilePath);
        var rdc = Rdc.Parse(stream);

        if (rdc.TryParse<LinkSprite>(stream, out var linkSprite))
            linkSprite?.Apply(bytes);

        if (rdc.TryParse<SamusSprite>(stream, out var samusSprite))
            samusSprite?.Apply(bytes);
    }
}
