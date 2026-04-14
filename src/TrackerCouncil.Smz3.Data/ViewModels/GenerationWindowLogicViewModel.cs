using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "Can Change Settings", visibleWhenTrue: nameof(CanChangeGameSettings))]
[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "All")]
public class GenerationWindowLogicViewModel : ViewModelBase
{
    [DynamicFormObject(groupName: "Can Change Settings")]
    public LogicConfig LogicConfig { get; set; } = new();

    [DynamicFormObject(groupName: "All")]
    public CasPatches CasPatches { get; set; } = new();

    public bool CanChangeGameSettings { get; set; } = true;

    public bool IsPlando
    {
        get;
        set
        {
            field = value;
            CasPatches.CanSetHintTiles = !value;
        }
    } = true;

    public bool SpinJumpAnimations
    {
        get;
        set
        {
            field = value;
            CasPatches.SpinJumpAnimationsDisabled = !value;
        }
    } = false;
}
