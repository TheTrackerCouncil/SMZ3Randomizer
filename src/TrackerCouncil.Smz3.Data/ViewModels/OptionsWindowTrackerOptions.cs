﻿using System.Collections.Generic;
using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using MSURandomizerLibrary;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Top")]
[DynamicFormGroupBasic(DynamicFormLayout.TwoColumns, "Bottom")]
public class OptionsWindowTrackerOptions
{
    [DynamicFormFieldColorPicker(label: "Tracker background color:")]
    public byte[] TrackerBGColor { get; set; } = [0xFF, 0x21, 0x21, 0x21];

    [DynamicFormFieldCheckBox(checkBoxText: "Render shadows", alignment: DynamicFormAlignment.Right)]
    public bool TrackerShadows { get; set; } = true;

    [DynamicFormFieldComboBox(label: "Tracker speech window image pack:",
        comboBoxOptionsProperty: nameof(TrackerSpeechImagePacks))]
    public string TrackerSpeechImagePack { get; set; } = "Default";

    [DynamicFormFieldColorPicker(label: "Tracker speech window color:")]
    public byte[] TrackerSpeechBGColor { get; set; } = [0xFF, 0x48, 0x3D, 0x8B];

    [DynamicFormFieldCheckBox(checkBoxText: "Enable speech bounce animation", alignment: DynamicFormAlignment.Right)]
    public bool TrackerSpeechEnableBounce { get; set; } = true;

    [DynamicFormFieldSlider(minimumValue: 0, maximumValue:100, decimalPlaces:1, incrementAmount:.1, suffix:"%", label: "Tracker recognition threshold:", platforms: DynamicFormPlatform.Windows)]
    public float TrackerRecognitionThreshold { get; set; }

    [DynamicFormFieldSlider(minimumValue: 0, maximumValue:100, decimalPlaces:1, incrementAmount:.1, suffix:"%", label: "Tracker execution threshold:", platforms: DynamicFormPlatform.Windows)]
    public float TrackerConfidenceThreshold { get; set; }

    [DynamicFormFieldSlider(minimumValue: 0, maximumValue:100, decimalPlaces:1, incrementAmount:.1, suffix:"%", label: "Tracker spoiler threshold:", platforms: DynamicFormPlatform.Windows)]
    public float TrackerConfidenceSassThreshold { get; set; }

    [DynamicFormFieldComboBox(label: "Tracker voice frequency:", platforms: DynamicFormPlatform.Windows)]
    public TrackerVoiceFrequency TrackerVoiceFrequency { get; set; }

    [DynamicFormFieldComboBox(label: "Speech recognition mode:", platforms: DynamicFormPlatform.Windows)]
    public SpeechRecognitionMode SpeechRecognitionMode { get; set; }

    [DynamicFormFieldComboBox(label: "Push-to-talk key:", platforms: DynamicFormPlatform.Windows)]
    public PushToTalkKey PushToTalkKey { get; set; }

    [DynamicFormFieldComboBox(label: "Push-to-talk device:", comboBoxOptionsProperty: nameof(AudioDevices), platforms: DynamicFormPlatform.Windows)]
    public string PushToTalkDevice { get; set; } = "";

    [DynamicFormFieldNumericUpDown(minValue: 0, label: "Undo expiration time:", platforms: DynamicFormPlatform.Windows)]
    public int UndoExpirationTime { get; set; } = 3;

    [DynamicFormFieldFilePicker(FilePickerType.Folder, label: "Auto tracker scripts folder:", dialogText: "Select auto tracker scripts folder")]
    public string AutoTrackerScriptsOutputPath { get; set; } = "";

    [DynamicFormFieldComboBox(label: "Auto tracker default connection:")]
    public SnesConnectorType ConnectorType { get; set; }

    [DynamicFormFieldTextBox(label: "QUSB2SNES IP address:")]
    public string Usb2SnesAddress { get; set; } = "";

    [DynamicFormFieldTextBox(label: "SNI IP address:")]
    public string SniAddress { get; set; } = "";

    [DynamicFormFieldComboBox(label: "Current song display style:")]
    public TrackDisplayFormat TrackDisplayFormat { get; set; } = TrackDisplayFormat.Vertical;

    [DynamicFormFieldFilePicker(FilePickerType.OpenFile, label: "Current song output path:", dialogText: "Select song output file")]
    public string? MsuTrackOutputPath { get; set; } = "";

    [DynamicFormFieldComboBox(label: "Auto tracker map update behavior")]
    public AutoMapUpdateBehavior AutoMapUpdateBehavior { get; set; }

    [DynamicFormFieldCheckBox(checkBoxText: "Auto track viewed events", groupName: "Bottom")]
    public bool AutoSaveLookAtEvents { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Enable hints", groupName: "Bottom", platforms: DynamicFormPlatform.Windows)]
    public bool TrackerHintsEnabled { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Enable spoilers", groupName: "Bottom", platforms: DynamicFormPlatform.Windows)]
    public bool TrackerSpoilersEnabled { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Enable timer", groupName: "Bottom")]
    public bool TrackerTimerEnabled { get; set; } = true;

    [DynamicFormFieldCheckBox(checkBoxText: "Enable MSU Randomizer message server", groupName: "Bottom", toolTipText: "Enables the gRPC server that allows the separate MSU Randomizer application from informing Tracker of when the MSU was shuffled and when the playing track is changed.")]
    public bool MsuMessageReceiverEnabled { get; set; } = true;

    public Dictionary<string, string> AudioDevices { get; set; } = new();
    public Dictionary<string, string> TrackerSpeechImagePacks { get; set; } = [];
}
