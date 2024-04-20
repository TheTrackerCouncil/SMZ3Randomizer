using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using Randomizer.Data.Logic;
using Randomizer.Data.Options;

namespace Randomizer.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "Non Plando", visibleWhenTrue: nameof(IsNotPlando))]
[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "All")]
public class GenerationWindowLogicViewModel : ViewModelBase
{
    [DynamicFormObject(groupName: "Non Plando")]
    public LogicConfig LogicConfig { get; set; } = new();

    [DynamicFormObject(groupName: "All")]
    public CasPatches CasPatches { get; set; } = new();

    public bool IsNotPlando { get; set; } = true;
}
