using System.Collections.Generic;
using Avalonia.Controls;
using AvaloniaControls.Models;
using ReactiveUI.SourceGenerators;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class SoloRomListViewModel : ViewModelBase
{
    public SoloRomListViewModel()
    {
    }

    [Reactive]
    [ReactiveLinkedProperties(nameof(HasRoms))]
    public partial List<GeneratedRomViewModel> Roms { get; set; } = new();

    public bool HasRoms => Roms.Count > 0;

    public Window? ParentWindow { get; set; }
}
