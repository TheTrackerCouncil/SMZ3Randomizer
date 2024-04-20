using System.Collections.Generic;
using System.Linq;
using AvaloniaControls.Models;
using Randomizer.Shared.Models;
using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class MultiRomListViewModel : ViewModelBase
{
    [Reactive] public List<MultiplayerRomViewModel> Games { get; set; } = [];

    [ReactiveLinkedProperties(nameof(Games))]
    public bool IsListVisible => Games.Count > 0;

    public void UpdateList(ICollection<MultiplayerGameDetails> details)
    {
        Games = details.Select(x => new MultiplayerRomViewModel(x)).ToList();
    }
}
