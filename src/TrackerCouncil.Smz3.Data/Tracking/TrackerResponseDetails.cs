using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Data.Tracking;

public class TrackerResponseLine
{
    public string SpeechText { get; init; } = "";
    public string DisplayText { get; init; } = "";
    public string? TrackerImage { get; init; } = null;
}

public class TrackerResponseDetails
{
    public required bool Successful { get; set; }
    public List<TrackerResponseLine>? Responses { get; set; }
}
