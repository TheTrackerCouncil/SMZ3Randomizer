﻿using System;
using System.Collections.Generic;
using System.Linq;
using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupExpander(DynamicFormLayout.Vertical, "Randomizer options", isExpanded: true)]
[DynamicFormGroupExpander(DynamicFormLayout.Vertical, "Tracker options")]
[DynamicFormGroupExpander(DynamicFormLayout.Vertical, "Twitch integration")]
[DynamicFormGroupExpander(DynamicFormLayout.Vertical, "Tracker profiles")]
public class OptionsWindowViewModel
{
    public OptionsWindowViewModel()
    {

    }

    public OptionsWindowViewModel(GeneralOptions options, Dictionary<string, string> trackerImagePacks, Dictionary<string, string> audioInputDevices,
        List<string> availableProfiles)
    {
        if (OperatingSystem.IsLinux() &&
            options.SpeechRecognitionMode is SpeechRecognitionMode.AlwaysOn or SpeechRecognitionMode.PushToTalk)
        {
            options.SpeechRecognitionMode = SpeechRecognitionMode.PySpeechService;
        }

        RandomizerOptions.Z3RomPath = options.Z3RomPath;
        RandomizerOptions.SMRomPath = options.SMRomPath;
        RandomizerOptions.RomOutputPath = options.RomOutputPath;
        RandomizerOptions.MsuPath = options.MsuPath;
        RandomizerOptions.LaunchButtonOption = options.LaunchButtonOption;
        RandomizerOptions.LaunchApplication = options.LaunchApplication;
        RandomizerOptions.LaunchArguments = options.LaunchArguments;
        RandomizerOptions.UIScaleFactor = options.UIScaleFactor;
        RandomizerOptions.CheckForUpdatesOnStartup = options.CheckForUpdatesOnStartup;
        RandomizerOptions.DownloadSpritesOnStartup = options.DownloadSpritesOnStartup;
        RandomizerOptions.DownloadConfigsOnStartup = options.DownloadConfigsOnStartup;

        TrackerOptions.TrackerBGColor = options.TrackerBGColor;
        TrackerOptions.TrackerShadows = options.TrackerShadows;
        TrackerOptions.TrackerSpeechImagePack = options.TrackerSpeechImagePack;
        TrackerOptions.TrackerSpeechBGColor = options.TrackerSpeechBGColor;
        TrackerOptions.TrackerSpeechEnableBounce = options.TrackerSpeechEnableBounce;
        TrackerOptions.TrackerRecognitionThreshold = options.TrackerRecognitionThreshold * 100;
        TrackerOptions.TrackerConfidenceThreshold = options.TrackerConfidenceThreshold * 100;
        TrackerOptions.TrackerConfidenceSassThreshold = options.TrackerConfidenceSassThreshold * 100;
        TrackerOptions.TextToSpeechVolume = options.TextToSpeechVolume;
        TrackerOptions.TrackerVoiceFrequency = options.TrackerVoiceFrequency;
        TrackerOptions.SpeechRecognitionMode = options.SpeechRecognitionMode.ToString();
        TrackerOptions.PushToTalkKey = options.PushToTalkKey;
        TrackerOptions.PushToTalkDevice = options.PushToTalkDevice;
        TrackerOptions.UndoExpirationTime = options.UndoExpirationTime;
        TrackerOptions.AutoTrackerScriptsOutputPath = options.AutoTrackerScriptsOutputPath;
        TrackerOptions.ConnectorType = options.SnesConnectorSettings.ConnectorType;
        TrackerOptions.Usb2SnesAddress = options.SnesConnectorSettings.Usb2SnesAddress;
        TrackerOptions.SniAddress = options.SnesConnectorSettings.SniAddress;
        TrackerOptions.TrackDisplayFormat = options.TrackDisplayFormat;
        TrackerOptions.MsuTrackOutputPath = options.MsuTrackOutputPath;
        TrackerOptions.AutoMapUpdateBehavior = options.AutoMapUpdateBehavior ?? AutoMapUpdateBehavior.Disabled;
        TrackerOptions.AutoSaveLookAtEvents = options.AutoSaveLookAtEvents;
        TrackerOptions.TrackerHintsEnabled = options.TrackerHintsEnabled;
        TrackerOptions.TrackerSpoilersEnabled = options.TrackerSpoilersEnabled;
        TrackerOptions.TrackerTimerEnabled = options.TrackerTimerEnabled;
        TrackerOptions.MsuMessageReceiverEnabled = options.MsuMessageReceiverEnabled;
        TrackerOptions.TrackerSpeechImagePacks = trackerImagePacks;
        TrackerOptions.AudioDevices = audioInputDevices;
        TrackerOptions.SpeechRecognitionTypes = GetSpeechRecognitionTypes();

        TwitchIntegration.TwitchUserName = options.TwitchUserName;
        TwitchIntegration.TwitchChannel = options.TwitchChannel;
        TwitchIntegration.TwitchId = options.TwitchId;
        TwitchIntegration.TwitchOAuthToken = options.TwitchOAuthToken;
        TwitchIntegration.EnableChatGreeting = options.EnableChatGreeting;
        TwitchIntegration.EnablePollCreation = options.EnablePollCreation;
        TwitchIntegration.ChatGreetingTimeLimit = options.ChatGreetingTimeLimit;
        TwitchIntegration.GanonsTowerGuessingGameStyle = options.GanonsTowerGuessingGameStyle;

        TrackerProfiles.AvailableProfiles = availableProfiles.Where(x => !string.IsNullOrEmpty(x) && !"Default".Equals(x)).ToList();
        TrackerProfiles.SelectedProfiles = options.SelectedProfiles.Where(x => TrackerProfiles.AvailableProfiles.Contains(x ?? "")).Cast<string>().Reverse().ToList();
    }

    public void UpdateOptions(GeneralOptions options)
    {
        var speechRecognitionMode = SpeechRecognitionMode.AlwaysOn;
        if (Enum.TryParse(TrackerOptions.SpeechRecognitionMode, out SpeechRecognitionMode parsedSpeechRecognitionMode))
        {
            speechRecognitionMode = parsedSpeechRecognitionMode;
        }

        options.Z3RomPath = RandomizerOptions.Z3RomPath;
        options.SMRomPath = RandomizerOptions.SMRomPath;
        options.RomOutputPath = RandomizerOptions.RomOutputPath;
        options.MsuPath = RandomizerOptions.MsuPath;
        options.LaunchButtonOption = RandomizerOptions.LaunchButtonOption;
        options.LaunchApplication = RandomizerOptions.LaunchApplication;
        options.LaunchArguments = RandomizerOptions.LaunchArguments;
        options.UIScaleFactor = RandomizerOptions.UIScaleFactor;
        options.CheckForUpdatesOnStartup = RandomizerOptions.CheckForUpdatesOnStartup;
        options.DownloadSpritesOnStartup = RandomizerOptions.DownloadSpritesOnStartup;
        options.DownloadConfigsOnStartup = RandomizerOptions.DownloadConfigsOnStartup;

        options.TrackerBGColor = TrackerOptions.TrackerBGColor;
        options.TrackerShadows = TrackerOptions.TrackerShadows;
        options.TrackerSpeechImagePack = TrackerOptions.TrackerSpeechImagePack;
        options.TrackerSpeechBGColor = TrackerOptions.TrackerSpeechBGColor;
        options.TrackerSpeechEnableBounce = TrackerOptions.TrackerSpeechEnableBounce;
        options.TrackerRecognitionThreshold = TrackerOptions.TrackerRecognitionThreshold / 100;
        options.TrackerConfidenceThreshold = TrackerOptions.TrackerConfidenceThreshold / 100;
        options.TrackerConfidenceSassThreshold = TrackerOptions.TrackerConfidenceSassThreshold / 100;
        options.TextToSpeechVolume = TrackerOptions.TextToSpeechVolume;
        options.TrackerVoiceFrequency = TrackerOptions.TrackerVoiceFrequency;
        options.SpeechRecognitionMode = speechRecognitionMode;
        options.PushToTalkKey = TrackerOptions.PushToTalkKey;
        options.PushToTalkDevice = TrackerOptions.PushToTalkDevice;
        options.UndoExpirationTime = TrackerOptions.UndoExpirationTime;
        options.AutoTrackerScriptsOutputPath = TrackerOptions.AutoTrackerScriptsOutputPath;
        options.SnesConnectorSettings.ConnectorType = TrackerOptions.ConnectorType;
        options.SnesConnectorSettings.Usb2SnesAddress = TrackerOptions.Usb2SnesAddress;
        options.SnesConnectorSettings.SniAddress = TrackerOptions.SniAddress;
        options.TrackDisplayFormat = TrackerOptions.TrackDisplayFormat;
        options.MsuTrackOutputPath = TrackerOptions.MsuTrackOutputPath;
        options.AutoMapUpdateBehavior = TrackerOptions.AutoMapUpdateBehavior;
        options.AutoSaveLookAtEvents = TrackerOptions.AutoSaveLookAtEvents;
        options.TrackerHintsEnabled = TrackerOptions.TrackerHintsEnabled;
        options.TrackerSpoilersEnabled = TrackerOptions.TrackerSpoilersEnabled;
        options.TrackerTimerEnabled = TrackerOptions.TrackerTimerEnabled;
        options.MsuMessageReceiverEnabled = TrackerOptions.MsuMessageReceiverEnabled;

        options.TwitchUserName = TwitchIntegration.TwitchUserName;
        options.TwitchChannel = TwitchIntegration.TwitchChannel;
        options.TwitchId = TwitchIntegration.TwitchId;
        options.TwitchOAuthToken = TwitchIntegration.TwitchOAuthToken;
        options.EnableChatGreeting = TwitchIntegration.EnableChatGreeting;
        options.EnablePollCreation = TwitchIntegration.EnablePollCreation;
        options.ChatGreetingTimeLimit = TwitchIntegration.ChatGreetingTimeLimit;
        options.GanonsTowerGuessingGameStyle = TwitchIntegration.GanonsTowerGuessingGameStyle;

        options.SelectedProfiles = TrackerProfiles.SelectedProfiles.Cast<string?>().Reverse().ToList();
    }

    [DynamicFormObject(groupName:"Randomizer options")]
    public OptionsWindowRandomizerOptions RandomizerOptions { get; set; } = new();

    [DynamicFormObject(groupName:"Tracker options")]
    public OptionsWindowTrackerOptions TrackerOptions { get; set; } = new();

    [DynamicFormObject(groupName:"Twitch integration")]
    public OptionsWindowTwitchIntegration TwitchIntegration { get; set; } = new();

    [DynamicFormObject(groupName:"Tracker profiles")]
    public OptionsWindowTrackerProfiles TrackerProfiles { get; set; } = new();

    private Dictionary<string, string> GetSpeechRecognitionTypes()
    {
        var toReturn = new Dictionary<string, string>();

        if (OperatingSystem.IsWindows())
        {
            toReturn[SpeechRecognitionMode.AlwaysOn.ToString()] = SpeechRecognitionMode.AlwaysOn.GetDescription();
            toReturn[SpeechRecognitionMode.PushToTalk.ToString()] = SpeechRecognitionMode.PushToTalk.GetDescription();
        }
        else if (OperatingSystem.IsLinux())
        {
            toReturn[SpeechRecognitionMode.PySpeechService.ToString()] = SpeechRecognitionMode.PySpeechService.GetDescription();
        }

        toReturn[SpeechRecognitionMode.Disabled.ToString()] = SpeechRecognitionMode.Disabled.GetDescription();

        return toReturn;
    }
}
