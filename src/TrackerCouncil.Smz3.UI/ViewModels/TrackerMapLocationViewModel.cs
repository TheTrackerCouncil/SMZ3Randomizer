using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using AvaloniaControls.Models;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class TrackerMapLocationViewModel : ViewModelBase
{
    private readonly int _baseX;
    private readonly int _baseY;
    private readonly int _yOffset;

    public TrackerMapLocationViewModel(TrackerMapRegion mapRegion, TrackerMapLocation mapLocation,
        TrackerMapLocation innerMapLocation, Region region, List<Location> locations)
    {
        Type = TrackerMapLocation.MapLocationType.Item;
        _baseX = (int)Math.Floor(innerMapLocation.X * mapLocation.Scale) + mapLocation.X;
        _baseY = (int)Math.Floor(innerMapLocation.Y * mapLocation.Scale) + mapLocation.Y;
        Name = innerMapLocation.Name;
        RegionName = mapRegion.Name;
        Region = region;
        Locations = locations.Select(x => new TrackerMapSubLocationViewModel { Name = $"Clear {x}", Location = x}).ToList();
    }

    public TrackerMapLocationViewModel(TrackerMapRegion mapRegion, TrackerMapLocation mapLocation, Region region)
    {
        Type = TrackerMapLocation.MapLocationType.Boss;
        _baseX = (int)Math.Floor((mapRegion.BossX ?? 0) * mapLocation.Scale) + mapLocation.X;
        _baseY = (int)Math.Floor((mapRegion.BossY ?? 0) * mapLocation.Scale) + mapLocation.Y;
        BossRegion = region as IHasBoss;
        RewardRegion = region as IHasReward;
        Name = BossRegion?.Boss.Name ?? region.Name;
        RegionName = mapRegion.Name;
        Region = region;

        if (RewardRegion != null)
        {
            _yOffset = -22;
        }
    }

    public TrackerMapLocationViewModel(TrackerMapRegion mapRegion, TrackerMapLocation mapLocation, Region region, TrackerMapSMDoor door)
    {
        Type = TrackerMapLocation.MapLocationType.SMDoor;
        _baseX = (int)Math.Floor(door.X * mapLocation.Scale) + mapLocation.X;
        _baseY = (int)Math.Floor(door.Y * mapLocation.Scale) + mapLocation.Y;
        Name = door.Item;
        RegionName = mapRegion.Name;
        Region = region;
    }

    public TrackerMapLocation.MapLocationType Type { get; }
    [Reactive] public string ImagePath { get; set; } = "";
    [Reactive] public string? NumberImagePath { get; set; }
    [Reactive] public string? MarkedImagePath { get; set; }
    public string RegionName { get; set; }
    public Region Region { get; set; }
    public IHasReward? RewardRegion { get; set; }
    public IHasBoss? BossRegion { get; set; }
    public string Name { get; set; }
    public List<TrackerMapSubLocationViewModel>? Locations { get; set; }
    public bool IconVisibility => HasImage && (IsInLogic || ShowOutOfLogic);
    [Reactive] public bool MarkedVisibility { get; set; } = true;
    [Reactive] public bool NumberVisibility { get; set; } = true;
    public int Size { get; set; } = 36;
    public double X => _baseX * Ratio - Size / 2.0 + Offset.Width;
    public double Y => _baseY * Ratio - Size / 2.0 + Offset.Height + _yOffset;
    public Size GridSize { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(X), nameof(Y))]
    public double Ratio { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(X), nameof(Y))]
    public Size Offset { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(IconVisibility))]
    public bool HasImage { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(IconVisibility))]
    public bool IsInLogic { get; set; } = true;

    [Reactive]
    [ReactiveLinkedProperties(nameof(IconVisibility))]
    public bool ShowOutOfLogic { get; set; }
}
