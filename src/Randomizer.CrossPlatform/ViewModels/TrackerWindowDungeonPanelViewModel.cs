using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class DungeonSetRewardEventArgs : EventArgs
{
    public required RewardType RewardType { get; init; }
}

public class TrackerWindowDungeonPanelViewModel : TrackerWindowPanelViewModel
{
    public IDungeon? Dungeon { get; set; }
    public string? DungeonImage { get; set; } = "";
    [Reactive] public string? RewardImage { get; set; }
    [Reactive] public bool DungeonCleared { get; set; }
    [Reactive] public int DungeonTreasure { get; set; }
    public event EventHandler? ResetCleared;
    public event EventHandler? TreasureCleared;
    public event EventHandler<DungeonSetRewardEventArgs>? RewardSet;

    public override List<TrackerWindowPanelImage?> GetImages()
    {
        var images = new List<TrackerWindowPanelImage?>
        {
            GetLabelImage(),
            GetRewardImage(),
            GetDungeonImage(),
        };

        if (DungeonTreasure > 0)
        {
            images.AddRange(GetNumberImages(DungeonTreasure, 1, 0, 16, false));
        }

        return images;
    }

    public override List<MenuItem> GetMenuItems()
    {
        var menuItems = new List<MenuItem>();

        if (DungeonCleared)
        {
            var menuItem = new MenuItem { Header = "Reset cleared status" };
            menuItem.Click += (_, _) => ResetCleared?.Invoke(this, EventArgs.Empty);
            menuItems.Add(menuItem);
        }

        if (DungeonTreasure > 0)
        {
            var menuItem = new MenuItem { Header = "Clear dungeon treasure" };
            menuItem.Click += (_, _) => TreasureCleared?.Invoke(this, EventArgs.Empty);
            menuItems.Add(menuItem);
        }

        if (Dungeon?.HasReward == true)
        {
            AddRewardMenuItem(menuItems, RewardType.PendantGreen);
            AddRewardMenuItem(menuItems, RewardType.PendantRed);
            AddRewardMenuItem(menuItems, RewardType.PendantBlue);
            AddRewardMenuItem(menuItems, RewardType.CrystalBlue);
            AddRewardMenuItem(menuItems, RewardType.CrystalRed);
        }

        return menuItems;
    }

    private void AddRewardMenuItem(List<MenuItem> menuItems, RewardType rewardType)
    {
        if (rewardType == Dungeon?.MarkedReward)
        {
            return;
        }
        var menuItem = new MenuItem { Header = $"Mark as {rewardType.GetDescription()}" };
        menuItem.Click += (_, _) => RewardSet?.Invoke(this, new DungeonSetRewardEventArgs { RewardType = rewardType });
        menuItems.Add(menuItem);
    }

    private TrackerWindowPanelImage? GetDungeonImage()
    {
        if (string.IsNullOrEmpty(DungeonImage))
        {
            return null;
        }

        return new TrackerWindowPanelImage
        {
            ImagePath = DungeonImage,
            IsActive = DungeonCleared,
            Width = 20,
            Height = 16
        };
    }

    private TrackerWindowPanelImage? GetRewardImage()
    {
        if (string.IsNullOrEmpty(RewardImage))
        {
            return null;
        }

        return new TrackerWindowPanelImage
        {
            ImagePath = RewardImage,
            IsActive = DungeonCleared
        };
    }
}
