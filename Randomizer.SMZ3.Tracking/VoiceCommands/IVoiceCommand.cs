using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public interface IVoiceCommand
    {
        Grammar BuildGrammar();
    }
}
