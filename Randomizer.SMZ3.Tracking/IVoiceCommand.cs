using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking
{
    public interface IVoiceCommand
    {
        Grammar BuildGrammar();
    }
}