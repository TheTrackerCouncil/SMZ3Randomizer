﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

using Microsoft.EntityFrameworkCore.Metadata.Conventions;

using Randomizer.SMZ3.Tracking;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// Represents user-configurable options for the general working of the
    /// randomizer itself.
    /// </summary>
    public class GeneralOptions : INotifyPropertyChanged
    {
        private string _twitchUserName;
        private string _twitchOAuthToken;
        private string _twitchChannel;
        private string _twitchId;

        /// <summary>
        /// Converts the enum descriptions into a string array for displaying in a dropdown
        /// </summary>
        public static IEnumerable<string> QuickLaunchOptions
        {
            get
            {
                var attributes = typeof(LaunchButtonOptions).GetMembers()
                    .SelectMany(member => member.GetCustomAttributes(typeof(DescriptionAttribute), true).Cast<DescriptionAttribute>())
                    .ToList();
                return attributes.Select(x => x.Description);
            }
        }

        public string Z3RomPath { get; set; }

        public string SMRomPath { get; set; }

        public string RomOutputPath { get; set; }
            = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "Seeds");

        public string AutotrackerScriptsOutputPath { get; set; }
            = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "AutotrackerScripts");

        [Range(0.0, 1.0)]
        public float TrackerRecognitionThreshold { get; set; } = 0.75f;

        [Range(0.0, 1.0)]
        public float TrackerConfidenceThreshold { get; set; } = 0.85f;

        [Range(0.0, 1.0)]
        public float TrackerConfidenceSassThreshold { get; set; } = 0.92f;

        public Color TrackerBackgroundColor { get; set; } = Color.FromRgb(0x21, 0x21, 0x21);

        public bool TrackerShadows { get; set; } = true;

        public int LaunchButton { get; set; } = (int)LaunchButtonOptions.PlayAndTrack;

        public bool TrackerHintsEnabled { get; set; }

        public bool TrackerSpoilersEnabled { get; set; }

        public bool AutotrackerAutoStart { get; set; }

        public string TwitchUserName
        {
            get => _twitchUserName;
            set
            {
                if (_twitchUserName != value)
                {
                    _twitchUserName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TwitchOAuthToken
        {
            get => _twitchOAuthToken;
            set
            {
                if (_twitchOAuthToken != value)
                {
                    _twitchOAuthToken = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TwitchChannel
        {
            get => _twitchChannel;
            set
            {
                var newValue = NormalizeTwitchChannel(value);
                if (_twitchChannel != newValue)
                {
                    _twitchChannel = newValue;
                    OnPropertyChanged();
                }
            }
        }

        public string TwitchId
        {
            get => _twitchId;
            set
            {
                if (_twitchId != value)
                {
                    _twitchId = value;
                    OnPropertyChanged();
                }
            }

        }

        public bool EnableChatGreeting { get; set; } = true;
        public bool EnablePollCreation { get; set; } = true;

        public int ChatGreetingTimeLimit { get; set; } = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Validate()
        {
            return File.Exists(Z3RomPath)
                && File.Exists(SMRomPath)
                && (Directory.Exists(RomOutputPath) || RomOutputPath == null);
        }

        public TrackerOptions GetTrackerOptions() => new()
        {
            MinimumRecognitionConfidence = TrackerRecognitionThreshold,
            MinimumExecutionConfidence = TrackerConfidenceThreshold,
            MinimumSassConfidence = TrackerConfidenceSassThreshold,
            HintsEnabled = TrackerHintsEnabled,
            SpoilersEnabled = TrackerSpoilersEnabled,
            UserName = TwitchChannel,
            ChatGreetingEnabled = EnableChatGreeting,
            ChatGreetingTimeLimit = ChatGreetingTimeLimit,
            PollCreationEnabled = EnablePollCreation
        };

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        private static string NormalizeTwitchChannel(string value)
        {
            try
            {
                if (value == null) return null;
                value = value.Trim();
                if (Uri.TryCreate(value, UriKind.Absolute, out var twitchUri)
                    && twitchUri.Host == "twitch.tv")
                {
                    return twitchUri.AbsolutePath.TrimStart('/');
                }
            }
            catch { }

            return value;
        }
    }

    /// <summary>
    /// Enum for the launch button options
    /// </summary>
    public enum LaunchButtonOptions
    {
        [Description("Play Rom and Open Tracker")]
        PlayAndTrack,
        [Description("Open Folder and Tracker")]
        OpenFolderAndTrack,
        [Description("Open Tracker")]
        TrackOnly,
        [Description("Play Rom")]
        PlayOnly,
        [Description("Open Folder")]
        OpenFolderOnly
    }
}
