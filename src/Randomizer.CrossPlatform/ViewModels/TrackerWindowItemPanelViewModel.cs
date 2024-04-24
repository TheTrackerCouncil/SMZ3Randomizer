using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class ItemChangedEventArgs : EventArgs
{
    public required Item Item { get; init; }
}

public class ItemDungeonRequirementEventArgs : EventArgs
{
    public required bool IsMMRequirement { get; init; }
    public required bool IsTRRequirement { get; init; }
}

public enum ItemDungeonRequirement
{
    None,
    MM,
    TR,
    Both
}

public class TrackerWindowItemPanelViewModel : TrackerWindowPanelViewModel
{
    public static Dictionary<ItemDungeonRequirement, string> RequirementImages = new();

    public Dictionary<Item, string>? Items { get; set; }

    public List<Item>? ConnectedItems { get; set; }

    [Reactive] public int ItemCount { get; set; }

    [Reactive] public int RetrievedItemCount { get; set; }

    [Reactive] public int Stage { get; set; }

    [Reactive] public bool IsMMRequirement { get; set; }

    [Reactive] public bool IsTRRequirement { get; set; }

    public bool IsMedallion { get; set; }

    public event EventHandler<ItemChangedEventArgs>? ItemGiven;

    public event EventHandler<ItemChangedEventArgs>? ItemRemoved;

    public event EventHandler<ItemDungeonRequirementEventArgs>? ItemSetAsDungeonRequirement;

    public void UpdateItem(Item? item, string? path)
    {
        if (Items != null && item != null && !string.IsNullOrEmpty(path))
        {
            Items[item] = path;
        }

        if (ConnectedItems != null)
        {
            ItemCount = ConnectedItems.Select(x => x.State).Distinct().Sum(x => x.TrackingState);
        }
        else if (Items != null)
        {
            ItemCount = Items.Keys.Sum(x => x.Counter);
        }

        IsLabelActive = Items?.Keys.Any(x => x.State.TrackingState > 0) == true;

        RetrievedItemCount = Items?.Keys.Count(x => x.State.TrackingState > 0) ?? 0;
        Stage = Items?.Keys.Sum(x => x.State.TrackingState) ?? 0;
    }

    public override List<TrackerWindowPanelImage?> GetImages()
    {
        var images = new List<TrackerWindowPanelImage?>
        {
            GetLabelImage()
        };

        if (Items != null)
        {
            foreach (var item in Items)
            {
                images.Add(new TrackerWindowPanelImage
                {
                    ImagePath = item.Value,
                    IsActive = item.Key.State.TrackingState > 0
                });

                images.AddRange(GetNumberImages(ItemCount, 2, 0, 16, RetrievedItemCount > 0));
            }
        }

        if (IsMedallion)
        {
            if (IsMMRequirement && IsTRRequirement)
            {
                images.Add(new TrackerWindowPanelImage
                {
                    ImagePath = RequirementImages[ItemDungeonRequirement.Both],
                    IsActive = Items?.Keys.First().State.TrackingState > 0,
                    Height = 16,
                    OffsetY = 16
                });
            }
            else if (IsMMRequirement)
            {
                images.Add(new TrackerWindowPanelImage
                {
                    ImagePath = RequirementImages[ItemDungeonRequirement.MM],
                    IsActive = Items?.Keys.First().State.TrackingState > 0,
                    Height = 16,
                    OffsetY = 16
                });
            }
            else if (IsTRRequirement)
            {
                images.Add(new TrackerWindowPanelImage
                {
                    ImagePath = RequirementImages[ItemDungeonRequirement.TR],
                    IsActive = Items?.Keys.First().State.TrackingState > 0,
                    Height = 16,
                    OffsetY = 16
                });
            }
        }

        return images;
    }

    public override List<MenuItem> GetMenuItems()
    {
        var menuItems = new List<MenuItem>();

        if (Items?.Count > 1)
        {
            foreach (var item in Items.Keys)
            {
                if (item.State.TrackingState == 0)
                {
                    var menuItem = new MenuItem { Header = $"Track {item.Name}" };
                    menuItem.Click += (_, _) => ItemGiven?.Invoke(this, new ItemChangedEventArgs { Item = item });
                    menuItems.Add(menuItem);
                }
                else
                {
                    var menuItem = new MenuItem { Header = $"Untrack {item.Name}" };
                    menuItem.Click += (_, _) => ItemRemoved?.Invoke(this, new ItemChangedEventArgs { Item = item });
                    menuItems.Add(menuItem);
                }
            }
        }
        else if (Items?.Count == 1)
        {
            var item = Items.Keys.First();
            if (item.State.TrackingState > 0)
            {
                var menuItem = new MenuItem { Header = $"Untrack {item.Name}" };
                menuItem.Click += (_, _) => ItemRemoved?.Invoke(this, new ItemChangedEventArgs { Item = item });
                menuItems.Add(menuItem);
            }
        }

        if (Items?.Keys.FirstOrDefault()?.Type is ItemType.Bombos or ItemType.Ether or ItemType.Quake)
        {
            var item = Items.Keys.First();

            var menuItem = new MenuItem { Header = "Not required for any dungeon" };
            menuItem.Click += (_, _) => ItemSetAsDungeonRequirement?.Invoke(this,
                new ItemDungeonRequirementEventArgs { IsMMRequirement = false, IsTRRequirement = false });
            menuItems.Add(menuItem);

            menuItem = new MenuItem { Header = "Required for Misery Mire" };
            menuItem.Click += (_, _) => ItemSetAsDungeonRequirement?.Invoke(this,
                new ItemDungeonRequirementEventArgs { IsMMRequirement = true, IsTRRequirement = false });
            menuItems.Add(menuItem);

            menuItem = new MenuItem { Header = "Required for Turtle Rock" };
            menuItem.Click += (_, _) => ItemSetAsDungeonRequirement?.Invoke(this,
                new ItemDungeonRequirementEventArgs { IsMMRequirement = false, IsTRRequirement = true });
            menuItems.Add(menuItem);

            menuItem = new MenuItem { Header = "Required for both" };
            menuItem.Click += (_, _) => ItemSetAsDungeonRequirement?.Invoke(this,
                new ItemDungeonRequirementEventArgs { IsMMRequirement = true, IsTRRequirement = true });
            menuItems.Add(menuItem);
        }

        return menuItems;
    }
}
