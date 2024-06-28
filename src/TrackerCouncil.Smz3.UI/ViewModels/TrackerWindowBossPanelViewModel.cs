using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class TrackerWindowBossPanelViewModel : TrackerWindowPanelViewModel
{
    public Boss? Boss { get; set; }
    public string BossImage { get; set; } = "";
    [Reactive] public bool BossDefeated { get; set; }
    public event EventHandler? BossRevived;

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

    public override List<MenuItem> GetMenuItems()
    {
        var menuItems = new List<MenuItem>();

        if (BossDefeated)
        {
            var menuItem = new MenuItem { Header = $"Revive {Boss?.Name}" };
            menuItem.Click += (_, _) => BossRevived?.Invoke(this, EventArgs.Empty);
            menuItems.Add(menuItem);
        }

        return menuItems;
    }
}
