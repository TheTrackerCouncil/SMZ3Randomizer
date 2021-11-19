using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using Randomizer.Shared.Models;

namespace Randomizer.App.ViewModels
{
    public class GeneratedRomsViewModel : INotifyPropertyChanged
    {
        public GeneratedRomsViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                RomsList = new()
                {
                    new(new GeneratedRom() { Label = "Test1", Date = DateTimeOffset.Now, Value = "12345", TrackerState = new TrackerState() { SecondsElapsed = 342.54 } }),
                    new(new GeneratedRom() { Date = DateTimeOffset.UtcNow, Value = "45623" }),
                    new(new GeneratedRom() { Label = "Test2", Date = DateTimeOffset.UtcNow, Value = "5634" }),
                    new(new GeneratedRom() { Date = DateTimeOffset.UtcNow, TrackerState = new TrackerState() { SecondsElapsed = 4245.64 } }),
                    new(new GeneratedRom() { Label = "Test3", Date = DateTimeOffset.UtcNow, Value = "4564656423" })
                };
            }
        }

        public GeneratedRomsViewModel(List<GeneratedRom> roms)
        {
            UpdateList(roms);
        }

        public void UpdateList(List<GeneratedRom> roms)
        {
            RomsList = roms.Select(x => new GeneratedRomViewModel(x)).ToList();
            OnPropertyChanged();
        }

        public List<GeneratedRomViewModel> RomsList { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new(""));
        }
    }

    public class GeneratedRomViewModel
    {
        public GeneratedRomViewModel(GeneratedRom rom)
        {
            Rom = rom;
        }

        public GeneratedRom Rom { get; }

        public string NameLabel => string.IsNullOrEmpty(Rom.Label) ? $"Seed: {Rom.Value}" : Rom.Label;

        public string SeedLabel => string.IsNullOrEmpty(Rom.Label) ? "" : $"Seed: {Rom.Value}";

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
                return $"Tracked Duration: {duration}";
            }
        }
    }

}
