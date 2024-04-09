using System.Collections;
using System.Collections.Generic;
using MSURandomizerLibrary;

namespace Randomizer.Data.ViewModels;

public class GenerationWindowBasicViewModel : ViewModelBase
{
    private ICollection<string> _presets;
    private string _presetName = "";
    private string _importString = "";
    private string _summary = "";
    private string _linkSpriteName = "";
    private string _samusSpriteName = "";
    private string _shipSpriteName = "";
    private string _linkSpritePath = "";
    private string _samusSpritePath = "";
    private string _shipSpritePath = "";
    private MsuRandomizationStyle? _msuRandomizationStyle;
    private MsuShuffleStyle _msuShuffleStyle;
    private List<string> _msuPaths = new();
    private string _msuText = "Vanilla Music";

    public ICollection<string> Presets
    {
        get => _presets;
        set => SetField(ref _presets, value);
    }

    public string PresetName
    {
        get => _presetName;
        set => SetField(ref _presetName, value);
    }

    public string ImportString
    {
        get => _importString;
        set => SetField(ref _importString, value);
    }

    public string Summary
    {
        get => _summary;
        set => SetField(ref _summary, value);
    }

    public string LinkSpriteName
    {
        get => _linkSpriteName;
        set => SetField(ref _linkSpriteName, value);
    }

    public string SamusSpriteName
    {
        get => _samusSpriteName;
        set => SetField(ref _samusSpriteName, value);
    }

    public string ShipSpriteName
    {
        get => _shipSpriteName;
        set => SetField(ref _shipSpriteName, value);
    }

    public string LinkSpritePath
    {
        get => _linkSpritePath;
        set => SetField(ref _linkSpritePath, value);
    }

    public string SamusSpritePath
    {
        get => _samusSpritePath;
        set => SetField(ref _samusSpritePath, value);
    }

    public string ShipSpritePath
    {
        get => _shipSpritePath;
        set => SetField(ref _shipSpritePath, value);
    }

    public MsuRandomizationStyle? MsuRandomizationStyle
    {
        get => _msuRandomizationStyle;
        set => SetField(ref _msuRandomizationStyle, value);
    }

    public MsuShuffleStyle MsuShuffleStyle
    {
        get => _msuShuffleStyle;
        set => SetField(ref _msuShuffleStyle, value);
    }

    public List<string> MsuPaths
    {
        get => _msuPaths;
        set => SetField(ref _msuPaths, value);
    }

    public string MsuText
    {
        get => _msuText;
        set => SetField(ref _msuText, value);
    }

    public void UpdateMsuDetails(List<string> msuPaths, MsuRandomizationStyle msuRandomizationStyle,
        MsuShuffleStyle msuShuffleStyle)
    {
        MsuPaths = msuPaths;
        MsuRandomizationStyle = msuRandomizationStyle;
        MsuShuffleStyle = msuShuffleStyle;
    }

}
