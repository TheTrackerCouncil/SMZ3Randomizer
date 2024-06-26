using System.Collections.Generic;
using System.Linq;
using AvaloniaControls.Models;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class MultiRomListViewModel : ViewModelBase
{
    [Reactive]
    [ReactiveLinkedProperties(nameof(IsListVisible))]
    public List<MultiplayerRomViewModel> Games { get; set; } = [];

    public bool IsListVisible => Games.Count > 0;

    public void UpdateList(ICollection<MultiplayerGameDetails> details)
    {
        Games = details.Select(x => new MultiplayerRomViewModel(x)).ToList();
    }
}
