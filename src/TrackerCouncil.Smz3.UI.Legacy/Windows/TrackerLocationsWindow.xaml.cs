﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.UI.Legacy.ViewModels;

namespace TrackerCouncil.Smz3.UI.Legacy.Windows;

/// <summary>
/// Interaction logic for TrackerLocationsWindow.xaml
/// </summary>
public partial class TrackerLocationsWindow : Window
{
    public TrackerLocationsWindow(TrackerLocationSyncer syncer, IUIService uiService)
    {
        DataContext = new TrackerViewModel(syncer, uiService);

        InitializeComponent();

        var sprite = uiService.GetSpritePath("Items", "chest.png", out _);
        if (string.IsNullOrEmpty(sprite)) throw new InvalidOperationException("Could not load chest sprite");
        ChestSprite = new BitmapImage(new Uri(sprite));

        sprite = uiService.GetSpritePath("Items", "key.png", out _);
        if (string.IsNullOrEmpty(sprite)) throw new InvalidOperationException("Could not load key sprite");
        KeySprite = new BitmapImage(new Uri(sprite));

        Smz3.UI.Legacy.App.RestoreWindowPositionAndSize(this);
    }

    public ImageSource ChestSprite { get; }

    public ImageSource KeySprite { get; }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Smz3.UI.Legacy.App.SaveWindowPositionAndSize(this);
    }
}
