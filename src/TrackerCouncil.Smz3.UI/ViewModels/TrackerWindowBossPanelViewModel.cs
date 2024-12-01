using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class TrackerWindowBossPanelViewModel : TrackerWindowPanelViewModel
{
    public Boss? Boss { get; set; }
    public string BossImage { get; set; } = "";
    public IHasReward? RewardRegion { get; set; }
    [Reactive] public string? RewardImage { get; set; }
    [Reactive] public bool BossDefeated { get; set; }
    public event EventHandler? BossRevived;
    public event EventHandler<DungeonSetRewardEventArgs>? RewardSet;

    public override List<TrackerWindowPanelImage> GetMainImages()
    {
        var images = new List<TrackerWindowPanelImage?>()
        {
            GetLabelImage(),
            new TrackerWindowPanelImage
            {
                ImagePath = BossImage,
                IsActive = BossDefeated,
                Height = 32,
                Width = 32,
                OffsetX = 0,
                OffsetY = 0
            }
        };

        return images.NonNull().ToList();
    }

    public override List<TrackerWindowPanelImage> GetOverlayImages()
    {
        if (RewardRegion == null)
        {
            return [];
        }
        else
        {
            return [GetRewardImage()];
        }
    }

    public override List<MenuItem> GetMenuItems()
    {
        var menuItems = new List<MenuItem>();

        if (BossDefeated)
        {
            var menuItem = new MenuItem { Header = $"Revive {Boss?.Name}" };
            menuItem.Click += (_, _) => BossRevived?.Invoke(this, EventArgs.Empty);
            menuItems.Add(menuItem);
        }

        if (RewardRegion != null)
        {
            AddRewardMenuItem(menuItems, RewardType.PendantGreen);
            AddRewardMenuItem(menuItems, RewardType.PendantRed);
            AddRewardMenuItem(menuItems, RewardType.PendantBlue);
            AddRewardMenuItem(menuItems, RewardType.CrystalBlue);
            AddRewardMenuItem(menuItems, RewardType.CrystalRed);
            AddRewardMenuItem(menuItems, RewardType.KraidToken);
            AddRewardMenuItem(menuItems, RewardType.PhantoonToken);
            AddRewardMenuItem(menuItems, RewardType.DraygonToken);
            AddRewardMenuItem(menuItems, RewardType.RidleyToken);
        }

        return menuItems;
    }

    private void AddRewardMenuItem(List<MenuItem> menuItems, RewardType rewardType)
    {
        if (RewardRegion == null || RewardRegion.MarkedReward == rewardType)
        {
            return;
        }
        var menuItem = new MenuItem { Header = $"Mark as {rewardType.GetDescription()}" };
        menuItem.Click += (_, _) => RewardSet?.Invoke(this, new DungeonSetRewardEventArgs { RewardType = rewardType });
        menuItems.Add(menuItem);
    }

    private TrackerWindowPanelImage GetRewardImage()
    {
        return new TrackerWindowPanelImage
        {
            ImagePath = RewardImage!,
            IsActive = BossDefeated
        };
    }
}
