using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using Randomizer.Data.Logic;
using Randomizer.Data.Options;

namespace Randomizer.Data.ViewModels;

public class GenerationWindowLogicViewModel : ViewModelBase
{
    [DynamicFormObject]
    public LogicConfig LogicConfig { get; set; } = new();

    [DynamicFormObject]
    public CasPatches CasPatches { get; set; } = new();
}
