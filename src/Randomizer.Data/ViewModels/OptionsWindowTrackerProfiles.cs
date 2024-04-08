using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;

namespace Randomizer.Data.ViewModels;

public class OptionsWindowTrackerProfiles : INotifyPropertyChanged
{
    private List<string> _availableProfiles = [];

    [DynamicFormFieldText]
    public string TrackerProfileDescription =>
        "All selected profiles in the right column will be combined in the order from top to bottom. In case of overrides, such as Tracker item images, the top most will be used.";

    [DynamicFormFieldEnableDisableReorder(nameof(AvailableProfiles))]
    public List<string> SelectedProfiles { get; set; } = new List<string>();

#pragma warning disable CS0067 // Event is never used
    [DynamicFormFieldButton("Refresh Profiles", DynamicFormAlignment.Right)]
    public event EventHandler? RefreshProfilesPressed;

    [DynamicFormFieldButton("Open Profile Folder", DynamicFormAlignment.Right)]
    public event EventHandler? OpenProfileFolderPressed;
#pragma warning restore CS0067 // Event is never used

    public List<string> AvailableProfiles
    {
        get => _availableProfiles;
        set => SetField(ref _availableProfiles, value);
    }

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
