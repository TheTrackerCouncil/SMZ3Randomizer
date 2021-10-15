using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class TrackPegCommand : IVoiceCommand
    {
        public event EventHandler<TrackerEventArgs>? PegPegged;

        public Grammar BuildGrammar()
        {
            var trackPegPhrase = new GrammarBuilder()
                .Append("Hey tracker, ")
                .Append(new Choices("track peg", "peg"));

            var grammar = trackPegPhrase.Build(nameof(TrackPegCommand));
            grammar.SpeechRecognized += Grammar_SpeechRecognized;
            return grammar;
        }

        protected virtual void OnPegPegged(TrackerEventArgs e)
            => PegPegged?.Invoke(this, e);

        private void Grammar_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            OnPegPegged(new TrackerEventArgs(e.Result.Confidence));
        }
    }
}
