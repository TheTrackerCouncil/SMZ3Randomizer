using System.ComponentModel;

namespace TrackerCouncil.Smz3.Data.Options;

public enum AutoMapUpdateBehavior
{
    [Description("Disabled")]
    Disabled,

    [Description("Update Map When Changing Games")]
    UpdateOnGameChange,

    [Description("Update Map When Changing Regions")]
    UpdateOnRegionChange,

    [Description("Update Map When Changing Games or Zelda Regions")]
    UpdateOnZeldaRegionChange,

    [Description("Update Map When Changing Games or Metroid Regions")]
    UpdateOnMetroidRegionChange,
}
