using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaControls.ControlServices;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class TrackerSpeechWindowService(ICommunicator communicator, OptionsFactory optionsFactory, TrackerSpriteService trackerSpriteService) : ControlService
{
    TrackerSpeechWindowViewModel _model = new();

    private DispatcherTimer _dispatcherTimer = new()
    {
        Interval = TimeSpan.FromSeconds(1.0 / 60),
    };

    private TrackerSpeechImagePack? _trackerSpeechImagePack;
    private TrackerSpeechReactionImages? _currentSpeechImages;
    private int _tickCount;
    private readonly int _maxTicks = 12;
    private readonly double _bounceHeight = 6;
    private bool _prevSpeaking;
    private bool _enableBounce;
    private bool _isValid;

    public TrackerSpeechWindowViewModel GetViewModel()
    {
        _trackerSpeechImagePack = trackerSpriteService.GetPack();

        if (_trackerSpeechImagePack == null)
        {
            return new TrackerSpeechWindowViewModel();
        }

        SetReactionType();

        var options = optionsFactory.Create();
        var bytes = options.GeneralOptions.TrackerSpeechBGColor;
        _enableBounce = options.GeneralOptions.TrackerSpeechEnableBounce;
        _model.Background = new SolidColorBrush(Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]));

        if (_currentSpeechImages == null)
        {
            return new TrackerSpeechWindowViewModel();
        }

        _model.TrackerImage = _currentSpeechImages.IdleImage;

        if (_enableBounce)
        {
            _model.AnimationMargin = new Thickness(0, 0, 0, -1 * _bounceHeight);

            _dispatcherTimer.Tick += (sender, args) =>
            {
                _tickCount++;
                var fraction = Math.Clamp(1.0 * _tickCount / _maxTicks, 0, 1);

                if (fraction < 0.5)
                {
                    _model.AnimationMargin = new Thickness(0, 0, 0, -1 * _bounceHeight + fraction * 2 * _bounceHeight);
                }
                else
                {
                    _model.AnimationMargin = new Thickness(0, 0, 0, (fraction - 0.5) * 2 * -1 * _bounceHeight);
                }

                if (fraction >= 1)
                {
                    _dispatcherTimer.Stop();
                }
            };
        }

        SaveOpenStatus(true);

        communicator.SpeakCompleted += Communicator_SpeakCompleted;
        communicator.VisemeReached += Communicator_VisemeReached;

        _isValid = true;

        return _model;
    }

    public void StopTimer()
    {
        _dispatcherTimer.Stop();
    }

    public void SaveOpenStatus(bool isOpen)
    {
        var options = optionsFactory.Create();
        if (options.GeneralOptions.DisplayTrackerSpeechWindow == isOpen)
        {
            return;
        }
        options.GeneralOptions.DisplayTrackerSpeechWindow = isOpen;
        options.Save();
    }

    private void Communicator_VisemeReached(object? sender, SpeakingUpdatedEventArgs e)
    {
        if (!_isValid)
        {
            return;
        }

        if (!_model.IsTrackerImageVisible)
        {
            _model.IsTrackerImageVisible = true;
        }

        SetReactionType(e.Request?.TrackerImage ?? "default");

        if (!e.IsSpeaking)
        {
            _model.TrackerImage = _currentSpeechImages?.IdleImage;
        }
        else
        {
            if (_enableBounce && !_prevSpeaking&& !_dispatcherTimer.IsEnabled)
            {
                _tickCount = 0;
                _dispatcherTimer.Start();
            }
            _model.TrackerImage = _currentSpeechImages?.TalkingImage;
        }

        _prevSpeaking = e.IsSpeaking;
    }

    private void Communicator_SpeakCompleted(object? sender, SpeakCompletedEventArgs e)
    {
        if (!_isValid)
        {
            return;
        }

        SetReactionType(e.SpeechRequest?.FollowedByBlankImage == true ? "blank" : "default");
        _model.TrackerImage = _currentSpeechImages?.IdleImage;
        _prevSpeaking = false;
    }

    private void SetReactionType(string reaction = "default")
    {
        if (_trackerSpeechImagePack == null)
        {
            return;
        }

        _currentSpeechImages = _trackerSpeechImagePack.GetReactionImages(reaction);
    }

    public string GetBackgroundHex()
    {
        var color = _model.Background.Color;
        return "#" + BitConverter.ToString([color.R, color.G, color.B]).Replace("-", string.Empty);
    }
}

