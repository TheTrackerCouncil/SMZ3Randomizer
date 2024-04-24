using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class TrackerWindowPanelViewModel : ViewModelBase
{
    public static Dictionary<int, string> NumberImagePaths = new();

    [Reactive] public bool IsLabelActive { get; set; }
    public string? LabelImage { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }

    public List<TrackerWindowPanelImage?> Images = [];

    public virtual List<TrackerWindowPanelImage?> GetImages()
    {
        return Images;
    }

    public event EventHandler? Clicked;

    public void Click()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    public virtual List<MenuItem> GetMenuItems()
    {
        return [];
    }

    protected TrackerWindowPanelImage? GetLabelImage()
    {
        if (string.IsNullOrEmpty(LabelImage))
        {
            return null;
        }

        return new TrackerWindowPanelImage { ImagePath = LabelImage, IsActive = IsLabelActive };
    }

    protected List<TrackerWindowPanelImage> GetNumberImages(int value, int minValue, int offsetX, int offsetY, bool isActive)
    {
        var images = new List<TrackerWindowPanelImage>();
        if (value < minValue)
        {
            return images;
        }

        var offset = 0;
        foreach (var digit in GetDigits(value))
        {
            if (!NumberImagePaths.TryGetValue(digit, out var sprite))
            {
                continue;
            }

            images.Add(new TrackerWindowPanelImage
            {
                ImagePath = sprite,
                Width = 10,
                Height = 14,
                OffsetX = offsetX + offset,
                OffsetY = offsetY,
                IsActive = isActive
            });

            offset += 8;
        }

        return images;
    }

    private static IEnumerable<int> GetDigits(int value)
    {
        var numDigits = value.ToString("0", CultureInfo.InvariantCulture).Length;
        for (var i = numDigits; i > 0; i--)
        {
            yield return value / (int)Math.Pow(10, i - 1) % 10;
        }
    }
}

public class TrackerWindowPanelImage
{
    public string ImagePath { get; set; } = "";
    public bool IsActive { get; set; }
    public int Width { get; set; } = 32;
    public int Height { get; set; } = 32;
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
}
