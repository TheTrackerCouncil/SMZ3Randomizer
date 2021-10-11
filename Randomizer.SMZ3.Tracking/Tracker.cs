using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace Randomizer.SMZ3.Tracking
{
    public class Tracker : IDisposable
    {
        private readonly SpeechSynthesizer _tts;
        private readonly SpeechRecognitionEngine _recognizer;
        private readonly Action<string> _log;
        private bool _disposed;

        public Tracker(Action<string> log)
        {
            _log = log;

            _tts = new SpeechSynthesizer();
            _tts.SelectVoiceByHints(VoiceGender.Female);

            var trackItemsCommand = new TrackItemsCommand();
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

        private void ItemTracked(object sender, ItemTrackedEventArgs e)
        {
            _log($"Recognized item {e.ItemType} with {e.Confidence:P2} confidence.");

            Progression.TrackItem(e.ItemType);
            Say($"Toggled {e.ItemType.GetDescription()} on.");
        }
    }
}
