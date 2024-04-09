using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using Randomizer.Data.Options;

namespace Randomizer.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Top")]
[DynamicFormGroupBasic(DynamicFormLayout.TwoColumns, "Bottom")]
[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "Bottom left", parentGroup: "Bottom")]
[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "Bottom right", parentGroup: "Bottom")]
public class OptionsWindowRandomizerOptions : INotifyPropertyChanged
{
    private string _romOutputPath = "";
    private string? _msuPath;
    private bool _displayMsuWarning;

    [DynamicFormFieldFilePicker(FilePickerType.OpenFile, labelText: "ALttP Japanese v1.0 ROM (required):",
        groupName: "Top", filter: "SNES ROMs (*.sfc, *.smc)|*.sfc;*.smc|All files (*.*)|*.*",
        dialogText: "Select 'A Link to the Past' ROM",
        checkSum: "03a63945398191337e896e5771f77173",
        checkSumError: "The rom selected does not appear to be a valid ALttP Japanese v1.0 ROM. Generated SMZ3 ROMs may not work as expected. Do you still wish to use it?")]
    public string? Z3RomPath { get; set; } = "";

    [DynamicFormFieldFilePicker(FilePickerType.OpenFile, labelText: "Super Metroid Japanese/US ROM (required):",
        groupName: "Top", filter: "SNES ROMs (*.sfc, *.smc)|*.sfc;*.smc|All files (*.*)|*.*",
        dialogText: "Select 'Super Metroid' ROM",
        checkSum: "21f3e98df4780ee1c667b84e57d88675",
        checkSumError: "The rom selected does not appear to be a valid Super Metroid Japanese/US ROM. Generated SMZ3 ROMs may not work as expected. Do you still wish to use it?")]
    public string? SMRomPath { get; set; } = "";

    [DynamicFormFieldFilePicker(FilePickerType.Folder, labelText: "ROM output folder:", groupName: "Top",
        dialogText: "Select ROM output folder")]
    public string RomOutputPath
    {
        get => _romOutputPath;
        set
        {
            SetField(ref _romOutputPath, value);
            ValidateMsuPath();
        }
    }

    [DynamicFormFieldFilePicker(FilePickerType.Folder, labelText: "MSU parent folder:", groupName: "Top",
        dialogText: "Select MSU parent folder")]
    public string? MsuPath
    {
        get => _msuPath;
        set
        {
            SetField(ref _msuPath, value);
            ValidateMsuPath();
        }
    }

    [DynamicFormFieldText(visibleWhenTrue: nameof(DisplayMsuWarning))]
    public string? MsuWarning =>
        "To preserve drive space and run faster, it is recommended that the Rom Output and MSU folders be on the same drive";

    public bool DisplayMsuWarning
    {
        get => _displayMsuWarning;
        set => SetField(ref _displayMsuWarning, value);
    }

    [DynamicFormFieldComboBox(labelText: "Launch button behavior:", groupName: "Top")]
    public LaunchButtonOptions LaunchButtonOption { get; set; }

    [DynamicFormFieldTextBox(labelText: "Launch application:")]
    public string? LaunchApplication { get; set; } = "";

    [DynamicFormFieldTextBox(labelText: "Launch arguments:")]
    public string? LaunchArguments { get; set; } = "";

    [DynamicFormFieldCheckBox(checkBoxText: "Check for updates on startup", groupName: "Bottom left")]
    public bool CheckForUpdatesOnStartup { get; set; }

    [DynamicFormFieldCheckBox(checkBoxText: "Download tracker config updates on startup", groupName: "Bottom left")]
    public bool DownloadConfigsOnStartup { get; set; }

    [DynamicFormFieldCheckBox(checkBoxText: "Download sprite updates on startup", groupName: "Bottom left")]
    public bool DownloadSpritesOnStartup { get; set; }

#pragma warning disable CS0067 // Event is never used
    [DynamicFormFieldButton(buttonText: "Update Configs", groupName: "Bottom right", alignment: DynamicFormAlignment.Right)]
    public event EventHandler? UpdateConfigButtonPressed;

    [DynamicFormFieldButton(buttonText: "Update Sprites", groupName: "Bottom right", alignment: DynamicFormAlignment.Right)]
    public event EventHandler? UpdateSpritesButtonPressed;
#pragma warning restore CS0067 // Event is never used

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

    private void ValidateMsuPath()
    {
        if (string.IsNullOrEmpty(RomOutputPath) || string.IsNullOrEmpty(MsuPath))
        {
            DisplayMsuWarning = false;
            return;
        }

        var outputDrive = new DriveInfo(RomOutputPath);
        var msuDrive = new DriveInfo(MsuPath);
        DisplayMsuWarning = outputDrive.Name != msuDrive.Name;
    }
}
