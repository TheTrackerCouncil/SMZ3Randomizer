using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class SoloRomListViewModel : ViewModelBase
{
    public SoloRomListViewModel()
    {
    }

    [Reactive]
    public List<GeneratedRomViewModel> Roms { get; set; } = new();
}
