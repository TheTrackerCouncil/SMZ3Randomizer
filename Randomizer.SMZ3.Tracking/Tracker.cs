using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;

using Randomizer.SMZ3.Tracking.VoiceCommands;

namespace Randomizer.SMZ3.Tracking
{
    public class Tracker : IDisposable
    {
        private static readonly Random s_random = new();

        private readonly SpeechSynthesizer _tts;
        private readonly SpeechRecognitionEngine _recognizer;
        private readonly Action<string> _log;
        private bool _disposed;

        public Tracker(Action<string> log, TrackerConfigProvider trackerConfigProvider)
        {
            _log = log;
            _tts = new SpeechSynthesizer();
            _tts.SelectVoiceByHints(VoiceGender.Female);

            var config = trackerConfigProvider.GetTrackerConfig();
            var trackItemsCommand = new TrackItemsCommand(config);
            trackItemsCommand.ItemTracked += ItemTracked;

            _recognizer = new SpeechRecognitionEngine();
            _recognizer.LoadGrammar(trackItemsCommand.BuildGrammar());
            _recognizer.SetInputToDefaultAudioDevice();
        }

        public virtual void StartTracking()
        {
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            Say("Is it weekend already?");
        }

        public virtual void StopTracking()
        {
            _recognizer.RecognizeAsyncStop();
            Say("Leaving so soon?", wait: true);
        }

        public virtual void Say(string text, bool wait = false)
        {
            if (wait)
                _tts.Speak(text);
            else
                _tts.SpeakAsync(text);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _recognizer.Dispose();
                    _tts.Dispose();
                }

                _disposed = true;
            }
        }

        private void ItemTracked(object? sender, ItemTrackedEventArgs e)
        {
            _log($"Recognized item {e.ItemData.Name} as {e.TrackedAs} with {e.Confidence:P2} confidence.");

            var randomName = e.ItemData.GetRandomName(s_random);

            if (e.ItemData.HasStages)
            {
                // TODO: Get current stage
                var stage = e.ItemData.GetStage(e.TrackedAs);
                if (stage != null)
                {
                    // Tracked by specific stage name (e.g. Tempered Sword), set
                    // to that stage specifically
                    var stageName = e.ItemData.Stages[stage.Value].Random(s_random);
                    e.ItemData.Track(stage.Value);
                    Say($"Marked {randomName} as {stageName}");
                }
                else
                {
                    // Tracked by regular name, upgrade by one step
                    if (e.ItemData.Track())
                    {
                        // Say($"Upgraded {randomName} by one step.");
                        var stageName = e.ItemData.Stages[e.ItemData.TrackingState].Random(s_random);
                        Say($"Congratulations with your new {stageName}.");
                    }
                    else
                    {
                        Say("I doubt that.");
                    }
                }
            }
            else if (e.ItemData.Multiple)
            {
                e.ItemData.Track();
                Say($"Added {randomName}.");
            }
            else
            {
                if (e.ItemData.Track())
                    Say($"Toggled {e.ItemData.Name} on.");
                else
                    Say("You already have one.");
            }
        }
    }
}
