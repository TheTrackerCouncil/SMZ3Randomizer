using System.ComponentModel;

namespace Randomizer.Data.Options;

public enum SpeechRecognitionMode
{
    [Description("Always on")]
    AlwaysOn,

    [Description("Push-to-talk")]
    PushToTalk,

    Disabled
}
