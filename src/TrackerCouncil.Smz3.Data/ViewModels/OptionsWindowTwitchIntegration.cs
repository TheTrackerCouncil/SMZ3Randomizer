using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.SideBySide)]
public class OptionsWindowTwitchIntegration : INotifyPropertyChanged
{

    private string _twitchStatusText = "";
    private bool _isLoggedIn;
    private string? _twitchUserName;
    private string? _twitchChannel;
    private string? _twitchId;

    [DynamicFormFieldTextBox(label: "Twitch user name:", order: 0, editableWhenTrue: nameof(CanEditTwitchText))]
    public string? TwitchUserName
    {
        get => _twitchUserName;
        set => SetField(ref _twitchUserName, value);
    }

    [DynamicFormFieldTextBox(label: "Twitch channel:", order: 10, editableWhenTrue: nameof(CanEditTwitchText))]
    public string? TwitchChannel
    {
        get => _twitchChannel;
        set => SetField(ref _twitchChannel, value);
    }

    [DynamicFormFieldTextBox(label: "Twitch id:", order: 20, editableWhenTrue: nameof(CanEditTwitchText))]
    public string? TwitchId
    {
        get => _twitchId;
        set => SetField(ref _twitchId, value);
    }

    public string? TwitchOAuthToken { get; set; } = "";

#pragma warning disable CS0067 // Event is never used
    [DynamicFormFieldButton("Log in with Twitch", DynamicFormAlignment.Right, order: 30, visibleWhenTrue: nameof(IsLoggedOut))]
    public event EventHandler? TwitchLoginPressed;

    [DynamicFormFieldButton("Log out", DynamicFormAlignment.Right, order: 30, visibleWhenTrue: nameof(IsLoggedIn))]
    public event EventHandler? TwitchLogoutPressed;
#pragma warning restore CS0067 // Event is never used

    [DynamicFormFieldText(DynamicFormAlignment.Right, order: 40)]
    public string TwitchStatusText
    {
        get => _twitchStatusText;
        set => SetField(ref _twitchStatusText, value);
    }

    [DynamicFormFieldCheckBox("Enable responding to chat", label: "Chat functionality:", order: 50)]
    public bool EnableChatGreeting { get; set; }

    [DynamicFormFieldCheckBox("Enable poll creation", order: 60)]
    public bool EnablePollCreation { get; set; }

    [DynamicFormFieldNumericUpDown(minValue: 0, label: "Chat greeting time period (in minutes):", groupName: "Bottom", order: 70, toolTipText: "How long before tracker will stop responding to messages from chat greeting her.")]
    public int ChatGreetingTimeLimit { get; set; }

    [DynamicFormFieldComboBox(label: "GT guessing game style:", order: 80)]
    public GanonsTowerGuessingGameStyle GanonsTowerGuessingGameStyle { get; set; }

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            _isLoggedIn = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLoggedOut));
        }
    }

    public bool IsLoggedOut => !_isLoggedIn;

    public bool CanEditTwitchText => false;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
