using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Data.ViewModels;
using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Options;

[DynamicFormGroupGroupBox(DynamicFormLayout.Vertical, "Cas' Patches")]
[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "Top", parentGroup: "Cas' Patches")]
[DynamicFormGroupBasic(DynamicFormLayout.TwoColumns, "Middle", parentGroup: "Cas' Patches")]
[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Bottom", parentGroup: "Cas' Patches")]
public class CasPatches : ViewModelBase
{
    [YamlIgnore]
    [DynamicFormFieldText(groupName: "Top")]
    public string Description => "Cas' Patches don't affect the logic, but make overall gameplay easier or accessible.";

    [DynamicFormFieldCheckBox(checkBoxText: "Disable flashing", toolTipText: "Disables many flashing effects from things like Ether, lightning, and Metroid bosses. Note some flashing may still exist unfortunately.\nMetroid patch developed by Kara.", groupName: "Middle")]
    public bool DisableFlashing { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Disable screen shake (Metroid)", toolTipText: "Disables the screen shake effect during escape and when lava/acid is rising.\nPatch developed by flo.", groupName: "Middle")]
    public bool DisableScreenShake { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Prevent Scams", toolTipText: "The bottle merchant and King Zora will tell you what they have before asking if you want to purchase it, even if you don't have enough rupees.", groupName: "Middle")]
    public bool PreventScams { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Randomized Bottles", toolTipText: "Adds randomly filled bottles instead of empty bottles and random fairy bottle trades.", groupName: "Middle")]
    public bool RandomizedBottles { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Aim with Any Button", toolTipText: "Allows you to map the aiming button to any button.\nPatch developed by Kejardon.", groupName: "Middle")]
    public bool AimAnyButton { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Infinite Space Jump", toolTipText: "Updates to make the space jump timing easier.\nPatch developed by MetConst.", groupName: "Middle")]
    public bool InfiniteSpaceJump { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Spin Jump Restart", toolTipText: "Allows you to initiate a spinning jump in the middle of falling normally.\nPatch developed by Kejardon.", groupName: "Middle")]
    public bool Respin { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Conserve Metroid Momentum", toolTipText: "Preserve your horizontal movement when landing from a fall or jump.\nPatch developed by Oi27.", groupName: "Middle")]
    public bool Speedkeep { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Starting Nerfed Charge Beam", toolTipText: "You will always start with a weak charge beam to prevent soft locks against bosses when running out of missiles.\nPatch developed by Smiley and Flo.", groupName: "Middle")]
    public bool NerfedCharge { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "No Bomb Torizo Softlocks", toolTipText: "Closes the door to Bomb Torizo faster after picking up the item to prevent softlocks by getting stuck in the door.", groupName: "Middle")]
    public bool NoBozoSoftlock { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Fast Metroid Doors", toolTipText: "Speeds up the transition between Metroid rooms via doors.\nPatch developed by Rakki.", groupName: "Middle")]
    public bool FastDoors { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Fast Metroid Elevators", toolTipText: "Speeds up the transition between Metroid areas via elevators.\nPatch developed by Lioran.", groupName: "Middle")]
    public bool FastElevators { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Refill at Save Stations", toolTipText: "Save stations will refill your ammo.\nPatch developed by Adam.", groupName: "Middle")]
    public bool RefillAtSaveStation { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Easier Wall Jumps", toolTipText: "Makes the timing of wall jumps more leniant.\nPatch developed by Benox50.", groupName: "Middle")]
    public bool EasierWallJumps { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Snap Morph to Holes", toolTipText: "Makes it easier to get into morph ball holes and allows you to morph over some 1 tile wide pits.\nPatch developed by Benox50.", groupName: "Middle")]
    public bool SnapMorph { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Auto Save on Metroid Deaths", toolTipText: "Save your progress when dying in Metroid. Does not apply to Start+Select+L+R resets.", groupName: "Middle")]
    public bool MetroidAutoSave { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Sand Pit Platforms", toolTipText: "Add platforms to make getting out of sand pits in Maridia easier.", groupName: "Middle")]
    public bool SandPitPlatforms { get; set; } = true;

    [DynamicFormFieldComboBox(label: "Zelda item drops:", groupName: "Bottom")]
    public ZeldaDrops ZeldaDrops { get; set; }

    [DynamicFormFieldSlider(0, 15, label: "Hint tiles:", groupName: "Bottom", visibleWhenTrue: nameof(CanSetHintTiles))]
    public int HintTiles { get; set; } = 15;

    [YamlIgnore] public bool CanSetHintTiles { get; set; } = true;

    public CasPatches Clone()
    {
        return (CasPatches)MemberwiseClone();
    }
}
