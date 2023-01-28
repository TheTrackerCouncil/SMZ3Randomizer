using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using Randomizer.Shared.Models;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// Class for the generated roms list window
    /// </summary>
    public class GeneratedRomsViewModel : INotifyPropertyChanged
    {
        public GeneratedRomsViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                RomsList = new()
                {
                    new(new GeneratedRom() { Label = "Test1", Date = DateTimeOffset.Now, Seed = "12345", TrackerState = new TrackerState() { SecondsElapsed = 342.54, PercentageCleared = 54 } }),
                    new(new GeneratedRom() { Date = DateTimeOffset.UtcNow, Seed = "45623" }),
                    new(new GeneratedRom() { Label = "Test2", Date = DateTimeOffset.UtcNow, Seed = "5634" }),
                    new(new GeneratedRom() { Date = DateTimeOffset.UtcNow, Seed = "234", TrackerState = new TrackerState() { SecondsElapsed = 4245.64, PercentageCleared = 20 } }),
                    new(new GeneratedRom() { Label = "Test3", Date = DateTimeOffset.UtcNow, Seed = "4564656423" })
                };
            }
            else
            {
                RomsList = new();
            }
        }

        public GeneratedRomsViewModel(List<GeneratedRom> roms)
        {
            RomsList = UpdateList(roms);
        }

        public List<GeneratedRomViewModel> UpdateList(List<GeneratedRom> roms)
        {
            RomsList = roms.Select(x => new GeneratedRomViewModel(x)).ToList();
            OnPropertyChanged();
            return RomsList;
        }

        public Visibility RomsListVisibility => RomsList.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility IntroVisibility => RomsList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        public List<GeneratedRomViewModel> RomsList { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new(""));
        }
    }

    /// <summary>
    /// Class for an individual entry for a generated rom
    /// </summary>
    public class GeneratedRomViewModel
    {
        public GeneratedRomViewModel(GeneratedRom rom)
        {
            Rom = rom;
        }

        public GeneratedRom Rom { get; }

        public string TextBoxName => $"EditLabelTextBox{Rom.Id}";
        public string Name => Rom.Label;

        public string NameLabel => string.IsNullOrEmpty(Rom.Label) ? $"Seed: {Rom.Seed}" : Rom.Label;

        public string LocationsLabel
        {
            get
            {
                if (Rom.TrackerState == null) return "";
                return $"Cleared: {Rom.TrackerState.PercentageCleared}%";
            }
        }

        public Visibility ProgressionLogVisibility => Rom.TrackerState?.History?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public string TimeLabel
        {
            get
            {
                return $"{Rom.Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern)} {Rom.Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern)}";
            }
        }

        public string ElapsedLabel
        {
            get
            {
                if (Rom.TrackerState == null) return "";
                var timeSpan = TimeSpan.FromSeconds(Rom.TrackerState.SecondsElapsed);
                var duration = timeSpan.Hours > 0
                    ? timeSpan.ToString("h':'mm':'ss")
                    : timeSpan.ToString("mm':'ss");
                return $"Duration: {duration}";
            }
        }
    }

}
