using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace Randomizer.SMZ3.Tracking
{
    public class Tracker : IDisposable
    {
        private static readonly Random s_random = new();

        private readonly SpeechSynthesizer _tts;
        private readonly SpeechRecognitionEngine _recognizer;
        private readonly Action<string> _log;
        private readonly TrackerConfigProvider _trackerConfigProvider;
        private bool _disposed;

        public Tracker(Action<string> log, TrackerConfigProvider trackerConfigProvider)
        {
            _log = log;
            _trackerConfigProvider = trackerConfigProvider;
            _tts = new SpeechSynthesizer();
            _tts.SelectVoiceByHints(VoiceGender.Female);

            var config = trackerConfigProvider.GetTrackerConfig();
            var trackItemsCommand = new TrackItemsCommand(config);
            trackItemsCommand.ItemTracked += ItemTracked;

            _recognizer = new SpeechRecognitionEngine();
            _recognizer.LoadGrammar(trackItemsCommand.BuildGrammar());
            _recognizer.SetInputToDefaultAudioDevice();
        }

        public Progression Progression { get; }
            = new Progression();

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
                    // Tracked by specific stage name (e.g. Tempered Sword), set to that stage specifically
                    var stageName = e.ItemData.Stages[stage.Value].Random(s_random);
                    Progression.TrackItem(e.ItemData.InternalItemType); // TODO: Set stage correctly
                    Say($"Marked {randomName} as {stageName}");
                }
                else
                {
                    // Tracked by regular name, upgrade by one step
                    Progression.TrackItem(e.ItemData.InternalItemType);
                    Say($"Upgraded {randomName} by one step.");
                }
            }
            else if (e.ItemData.Multiple)
            {
                Progression.TrackItem(e.ItemData.InternalItemType);
                Say($"Added {randomName}.");
            }
            else
            {
                Progression.TrackItem(e.ItemData.InternalItemType);
                Say($"Toggled {e.ItemData.Name} on.");
            }
        }
    }
}
