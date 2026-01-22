using System.Collections.Generic;
using AvaloniaControls.Models;
using ReactiveUI.SourceGenerators;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Multiplayer;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class MultiplayerConnectWindowViewModel : ViewModelBase
{
    public readonly List<string> DefaultServers = new()
    {
        "https://smz3.celestialrealm.net",
#if DEBUG
        "http://192.168.50.100:5000",
        "http://localhost:5000"
#endif
    };

    public bool IsCreatingGame { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(CanPressButton))]
    public partial string Url { get; set; } = "";

    [Reactive]
    [ReactiveLinkedProperties(nameof(CanPressButton))]
    public partial string DisplayName { get; set; } = "";

    [Reactive] public partial string PhoneticName { get; set; } = "";

    [Reactive] public partial MultiplayerGameType MultiplayerGameType { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(CanEnterInput), nameof(StatusText), nameof(CanPressButton))]
    public partial bool IsConnecting { get; set; }

    [Reactive] public partial bool AsyncGame { get; set; }

    [Reactive] public partial bool DeathLink { get; set; }

    [Reactive] public partial bool SendItemsOnComplete { get; set; }

    public string StatusText => IsConnecting ? "Connecting..." : "";
    public bool CanEnterInput => !IsConnecting;
    public string UrlLabelText => IsCreatingGame ? "Server url:" : "Game url:";
    public bool CanPressButton => DisplayName.Length > 0 && Url.Length > 0 && !IsConnecting;
    public string GameButtonText => IsCreatingGame ? "Create Game" : "Join Game";

    public string ServerUrl => Url.SubstringBeforeCharacter('?') ?? Url;
    public string GameGuid => Url.SubstringAfterCharacter('=') ?? "";
}
