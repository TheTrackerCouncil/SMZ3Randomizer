using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "Can Change Settings", visibleWhenTrue: nameof(CanChangeGameSettings))]
[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "All")]
public class GenerationWindowLogicViewModel : ViewModelBase
{
    private bool _isPlando = true;

    [DynamicFormObject(groupName: "Can Change Settings")]
    public LogicConfig LogicConfig { get; set; } = new();

    [DynamicFormObject(groupName: "All")]
    public CasPatches CasPatches { get; set; } = new();

    public bool CanChangeGameSettings { get; set; } = true;

    public bool IsPlando
    {
        get => _isPlando;
        set
        {
            _isPlando = value;
            CasPatches.CanSetHintTiles = !value;
        }
    }
}
