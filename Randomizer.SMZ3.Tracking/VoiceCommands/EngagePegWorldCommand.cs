using System;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class EngagePegWorldCommand : IVoiceCommand
    {
        public event EventHandler<TrackerEventArgs>? PegWorldModeStarted;

        public Grammar BuildGrammar()
        {
            var startPegWorldMode = new GrammarBuilder()
                .Append("Hey tracker, ")
                .Append(new Choices("we're going to Peg World!", "let's go to Peg World!"));

            var grammar = startPegWorldMode.Build(nameof(EngagePegWorldCommand));
            grammar.SpeechRecognized += Grammar_SpeechRecognized;
            return grammar;
        }

        protected virtual void OnPegWorldModeStarted(TrackerEventArgs e)
            => PegWorldModeStarted?.Invoke(this, e);

        private void Grammar_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            OnPegWorldModeStarted(new TrackerEventArgs(e.Result.Confidence));
        }
    }
}
