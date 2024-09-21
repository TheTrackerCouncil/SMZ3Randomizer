using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaControls.ControlServices;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Tracking;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class TrackerSpeechWindowService(ICommunicator communicator, IUIService uiService, OptionsFactory optionsFactory) : ControlService
{
    TrackerSpeechWindowViewModel _model = new();

    private DispatcherTimer _dispatcherTimer = new()
    {
        Interval = TimeSpan.FromSeconds(1.0 / 60),
    };

    private TrackerSpeechImages? _currentSpeechImages;
    private Dictionary<string, TrackerSpeechImages> _availableSpeechImages = [];
    private int _tickCount;
    private readonly int _maxTicks = 12;
    private readonly double _bounceHeight = 6;
    private int _prevViseme;
    private bool _enableBounce;
    private string? _currentReactionType;

    public TrackerSpeechWindowViewModel GetViewModel()
    {
        _availableSpeechImages = uiService.GetTrackerSpeechSprites(out _);
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

    private void Communicator_VisemeReached(object? sender, SpeakVisemeReachedEventArgs e)
    {
        if (!OperatingSystem.IsWindows()) return;

        SetReactionType(e.Request.TrackerImage);

        if (e.VisemeDetails.Viseme == 0)
        {
            _model.TrackerImage = _currentSpeechImages?.IdleImage;
        }
        else
        {
            if (_enableBounce && _prevViseme == 0 && !_dispatcherTimer.IsEnabled)
            {
                _tickCount = 0;
                _dispatcherTimer.Start();
            }
            _model.TrackerImage = _currentSpeechImages?.TalkingImage;
        }

        _prevViseme = e.VisemeDetails.Viseme;
    }

    private void Communicator_SpeakCompleted(object? sender, SpeakCompletedEventArgs e)
    {
        SetReactionType();
        _model.TrackerImage = _currentSpeechImages?.IdleImage;
        _prevViseme = 0;
    }

    private void SetReactionType(string reaction = "default")
    {
        var newReactionType = reaction.ToLower();

        if (_currentReactionType == newReactionType)
        {
            return;
        }

        if (_availableSpeechImages.TryGetValue(newReactionType, out var requestedSpeechImage))
        {
            _currentReactionType = newReactionType;
            _currentSpeechImages = requestedSpeechImage;
        }
        else if (_availableSpeechImages.TryGetValue("default", out var defaultSpeechImage))
        {
            _currentReactionType = "default";
            _currentSpeechImages = defaultSpeechImage;
        }
        else
        {
            var newPair = _availableSpeechImages.FirstOrDefault();
            _currentReactionType = newPair.Key.ToLower();
            _currentSpeechImages = newPair.Value;
        }
    }

    public string GetBackgroundHex()
    {
        var color = _model.Background.Color;
        return "#" + BitConverter.ToString([color.R, color.G, color.B]).Replace("-", string.Empty);
    }
}

