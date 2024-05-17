using System.Collections.Generic;

namespace Randomizer.CrossPlatform.ViewModels;

public class TrackerHelpWindowViewModel : ViewModelBase
{
    public TrackerHelpWindowViewModel()
    {

    }

    public List<TrackerHelpWindowSyntaxItem> SpeechRecognitionSyntax { get; init; } = [];
}

public class TrackerHelpWindowSyntaxItem
{
    public string Key { get; set; } = "";
    public List<string> Values { get; set; } = [];
}
