using System.ComponentModel;

namespace TrackerCouncil.Smz3.Data.Options;

public enum SpeechRecognitionMode
{
    [Description("Always on")]
    AlwaysOn,

    [Description("Push-to-talk")]
    PushToTalk,

    [Description("PySpeechService application")]
    PySpeechService,

    Disabled
}
