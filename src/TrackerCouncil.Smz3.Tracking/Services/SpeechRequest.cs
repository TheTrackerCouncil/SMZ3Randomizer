namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Request that is sent to the ICommunicator
/// </summary>
/// <param name="text">The plain text or SSML representation of the text to communicate.</param>
/// <param name="trackerImage">The tracker image to be displayed</param>
/// <param name="wait">If the communicator should block the calling thread</param>
public class SpeechRequest(string text, string? trackerImage = null, bool wait = false)
{
    /// <summary>
    /// The plain text or SSML representation of the text to communicate.
    /// </summary>
    public string Text => text;

    /// <summary>
    /// The tracker image that should be shown
    /// </summary>
    public string TrackerImage => trackerImage ?? "default";

    /// <summary>
    /// If the communicator should block the calling thread
    /// </summary>
    public bool Wait => wait;

    /// <summary>
    /// If the next tracker image is a blank one
    /// </summary>
    public bool FollowedByBlankImage { get; set; }
}
