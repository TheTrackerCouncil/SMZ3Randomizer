using System.Linq;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class TrackerHelpWindowService(TrackerBase trackerBase) : ControlService
{
    public TrackerHelpWindowViewModel GetViewModel()
    {
        return new TrackerHelpWindowViewModel
        {
            SpeechRecognitionSyntax = trackerBase.Syntax.Select(x => new TrackerHelpWindowSyntaxItem { Key = x.Key, Values = x.Value.ToList() }).ToList()
        };
    }
}
