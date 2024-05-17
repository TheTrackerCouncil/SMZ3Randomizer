using System.Linq;
using AvaloniaControls.ControlServices;
using Randomizer.Abstractions;
using Randomizer.CrossPlatform.ViewModels;

namespace Randomizer.CrossPlatform.Services;

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
