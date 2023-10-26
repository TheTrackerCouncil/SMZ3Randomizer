using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared.Enums;
using YamlDotNet.Serialization;

namespace Randomizer.Data.Options;

public class KeysanityConfig : GameModeConfig
{
    public KeysanityMode KeysanityMode { get; set; }

    [YamlIgnore]
    public bool KeysanityEnabled => Enabled && KeysanityMode != KeysanityMode.None;

    [YamlIgnore]
    public bool ZeldaKeysanity => Enabled && KeysanityMode is KeysanityMode.Zelda or KeysanityMode.Both;

    [YamlIgnore]
    public bool MetroidKeysanity => Enabled && KeysanityMode is KeysanityMode.SuperMetroid or KeysanityMode.Both;

    public bool KeysanityForRegion(Region region) => KeysanityMode == KeysanityMode.Both ||
                                                     (region is Z3Region && ZeldaKeysanity) ||
                                                     (region is SMRegion && MetroidKeysanity);
}
