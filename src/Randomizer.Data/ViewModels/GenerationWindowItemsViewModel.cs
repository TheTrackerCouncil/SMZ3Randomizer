using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;

namespace Randomizer.Data.ViewModels;

public class GenerationWindowItemsViewModel : ViewModelBase
{
    private List<GenerationWindowLocationViewModel> _locations;

    public IDictionary<LocationId, int> LocationItems { get; set; } = new Dictionary<LocationId, int>();

    public List<GenerationWindowItemOptionsViewModel> MetroidItemOptions { get; set; } = new();

    public List<GenerationWindowItemOptionsViewModel> ZeldaItemOptions { get; set; } = new();

    public Dictionary<string, Region?> Regions { get; set; } = new();

    public string SelectedRegionName { get; set; } = "";

    public Region? SelectedRegion => Regions[SelectedRegionName];

    public List<LocationItemOption> LocationItemOptions = new();

    public List<GenerationWindowLocationViewModel> LocationOptions { get; set; } = new();

    public List<GenerationWindowLocationViewModel> CurrentLocationOptions
    {
        get => _locations;
        set => SetField(ref _locations, value);
    }

    public void UpdateLocationOptions(Region? region)
    {
        CurrentLocationOptions = LocationOptions.Where(x => x.Region.Name == region?.Name).ToList();
    }

    public void Init(RandomizerOptions options, LocationConfig locations)
    {
        MetroidItemOptions = ItemSettingOptions.GetOptions()
            .Where(x => x.IsMetroid)
            .OrderBy(x => x.Item)
            .Select(x =>
                new GenerationWindowItemOptionsViewModel()
                {
                    Title = x.Item,
                    Options = x.Options,
                    SelectedOption = options.SeedOptions.ItemOptions.TryGetValue(x.Item, out var option) ? x.Options[option] : x.Options.First(),
                    IsMetroid = x.IsMetroid
                }).ToList();

        ZeldaItemOptions = ItemSettingOptions.GetOptions()
            .Where(x => !x.IsMetroid)
            .OrderBy(x => x.Item)
            .Select(x =>
                new GenerationWindowItemOptionsViewModel()
                {
                    Title = x.Item,
                    Options = x.Options,
                    SelectedOption = options.SeedOptions.ItemOptions.TryGetValue(x.Item, out var option) ? x.Options[option] : x.Options.First(),
                    IsMetroid = x.IsMetroid
                }).ToList();

        var world = new World(new(), "", 0, "");

        // Populate the regions filter dropdown
        Regions.Add("", null);
        foreach (var region in world.Regions.OrderBy(x => x is Z3Region))
        {
            var name = $"{(region is Z3Region ? "Zelda" : "Metroid")} - {region.Name}";
            Regions.Add(name, region);
        }

        // Add generic item placement options (Any, Progressive Items, Junk)
        foreach (var itemPlacement in Enum.GetValues<ItemPool>())
        {
            var description = itemPlacement.GetDescription();
            LocationItemOptions.Add(new LocationItemOption { Value = (int)itemPlacement, Text = description });
        }

        // Add specific items
        foreach (var itemType in Enum.GetValues<ItemType>())
        {
            if (itemType.IsInCategory(ItemCategory.NonRandomized) || itemType == ItemType.Nothing) continue;
            var description = itemType.GetDescription();
            LocationItemOptions.Add(new LocationItemOption { Value = (int)itemType, Text = description });
        }

        // Create rows for each location to be able to specify the items at
        // that location
        foreach (var location in world.Locations.OrderBy(x => x.Room == null ? "" : x.Room.Name).ThenBy(x => x.Name))
        {
            var locationDetails = locations.Single(x => x.LocationNumber == (int)location.Id);
            LocationOptions.Add(new GenerationWindowLocationViewModel()
            {
                LocationId = location.Id,
                LocationName = locationDetails.Name?.FirstOrDefault() ?? location.Id.ToString(),
                Options = LocationItemOptions,
                Region = location.Region,
                SelectedOption = options.SeedOptions.LocationItems.TryGetValue(location.Id, out var locationSetting)
                    ? (LocationItemOptions.FirstOrDefault(x => x.Value == locationSetting) ?? LocationItemOptions.First())
                    : LocationItemOptions.First()
            });
        }
    }

    public void ApplyConfig(Config config)
    {
        foreach (var item in MetroidItemOptions)
        {
            if (config.ItemOptions.TryGetValue(item.Title, out var option))
            {
                item.SelectedOption = item.Options[option];
            }
            else
            {
                item.SelectedOption = item.Options.First();
            }
        }

        foreach (var item in ZeldaItemOptions)
        {
            if (config.ItemOptions.TryGetValue(item.Title, out var option))
            {
                item.SelectedOption = item.Options[option];
            }
            else
            {
                item.SelectedOption = item.Options.First();
            }
        }

        foreach (var location in LocationOptions)
        {
            if (config.LocationItems.TryGetValue(location.LocationId, out var option))
            {
                location.SelectedOption =
                    location.Options.FirstOrDefault(x => x.Value == option) ?? location.Options.First();
            }
            else
            {
                location.SelectedOption = location.Options.First();
            }
        }
    }

    public void ApplySettings(RandomizerOptions options)
    {
        options.SeedOptions.ItemOptions = MetroidItemOptions.Concat(ZeldaItemOptions)
            .Where(x => x.SelectedOption != x.Options.First())
            .ToDictionary(x => x.Title, x => x.Options.IndexOf(x.SelectedOption));

        options.SeedOptions.LocationItems = LocationOptions.Where(x => x.SelectedOption != x.Options.First())
            .ToDictionary(x => x.LocationId, x => x.SelectedOption.Value);
    }
}

public class GenerationWindowItemOptionsViewModel : ViewModelBase
{
    private ItemSettingOption _selectedOption;

    public string Title { get; set; }
    public List<ItemSettingOption> Options { get; set; }

    public ItemSettingOption SelectedOption
    {
        get => _selectedOption;
        set => SetField(ref _selectedOption, value);
    }

    public bool IsMetroid { get; set; }
}

public class GenerationWindowLocationViewModel : ViewModelBase
{
    private LocationItemOption _selectedOption;

    public string LocationName { get; set; }
    public LocationId LocationId { get; set;}
    public Region Region { get; set; }
    public List<LocationItemOption> Options { get; set; }

    public LocationItemOption SelectedOption
    {
        get => _selectedOption;
        set => SetField(ref _selectedOption, value);
    }
}

public class LocationItemOption
{
    public int Value { get; init; }
    public string Text { get; init; } = "";

    public override string ToString() => Text;
}
