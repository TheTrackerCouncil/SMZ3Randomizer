using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Randomizer.Abstractions;
using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.AutoTracking;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.CrossPlatform;

public class ConsoleTrackerDisplayService
{
    private readonly System.Timers.Timer _timer;
    private readonly Smz3GeneratedRomLoader _romLoaderService;
    private readonly TrackerOptionsAccessor _trackerOptionsAccessor;
    private readonly RandomizerOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private TrackerBase _trackerBase = null!;
    private World _world = null!;
    private IWorldService _worldService = null!;
    private Region _lastRegion = null!;

    public ConsoleTrackerDisplayService(IServiceProvider serviceProvider, Smz3GeneratedRomLoader romLoaderService, TrackerOptionsAccessor trackerOptionsAccessor, OptionsFactory optionsFactory)
    {
        _romLoaderService = romLoaderService;
        _trackerOptionsAccessor = trackerOptionsAccessor;
        _options = optionsFactory.Create();
        _serviceProvider = serviceProvider;
        _timer = new System.Timers.Timer(TimeSpan.FromMilliseconds(250));
        _timer.Elapsed += delegate { UpdateScreen(); };
    }

    public async Task StartTracking(GeneratedRom rom, string romPath)
    {
        _trackerOptionsAccessor.Options = _options.GeneralOptions.GetTrackerOptions();
        _world = _romLoaderService.LoadGeneratedRom(rom).First(x => x.IsLocalWorld);
        _worldService = _serviceProvider.GetRequiredService<IWorldService>();
        _trackerBase = _serviceProvider.GetRequiredService<TrackerBase>();
        _trackerBase.Load(rom, romPath);
        _trackerBase.TryStartTracking();
        _trackerBase.AutoTracker?.SetConnector(_options.AutoTrackerDefaultConnector, _options.AutoTrackerQUsb2SnesIp);

        if (_trackerBase.AutoTracker != null)
        {
            _trackerBase.AutoTracker.AutoTrackerConnected += delegate
            {
                UpdateScreen();
                if (!_timer.Enabled)
                {
                    _timer.Start();
                }
            };

            _trackerBase.LocationCleared += delegate(object? _, LocationClearedEventArgs args)
            {
                _lastRegion = args.Location.Region;
            };
        }

        while (true)
        {
            Console.ReadKey();
            _timer.Stop();
            Console.Clear();
            Console.Write("Do you want to quit? (y/n) ");
            var response = Console.ReadLine();

            if ("y".Equals(response, StringComparison.OrdinalIgnoreCase))
            {
                await _trackerBase.SaveAsync();
                break;
            }

            _timer.Start();
            UpdateScreen();
        }
    }

    private void UpdateScreen()
    {
        var columnWidth = (Console.WindowWidth-6)/2;

        var topLines = GetHeaderLines(columnWidth);

        var leftColumnLines = new List<string>();
        leftColumnLines.AddRange(GetInventoryLines(columnWidth));
        leftColumnLines.Add("");
        leftColumnLines.AddRange(GetDungeonLines(columnWidth));

        var rightColumnLines = new List<string>();
        rightColumnLines.AddRange(GetLocationLines(columnWidth));

        while (leftColumnLines.Count < Console.WindowHeight - 1)
        {
            leftColumnLines.Add("");
        }

        while (rightColumnLines.Count < Console.WindowHeight - 1)
        {
            rightColumnLines.Add("");
        }

        var sb = new StringBuilder();

        foreach (var line in topLines)
        {
            sb.AppendLine(line);
        }

        for (var i = 0; i < leftColumnLines.Count && i < Console.WindowHeight - topLines.Count - 1; i++)
        {
            // Retrieve and pad left column, trimming if needed
            var leftColumn = leftColumnLines[i];
            if (leftColumn.Length > columnWidth)
            {
                leftColumn = leftColumn[..(columnWidth - 3)] + "...     ";
            }
            else
            {
                leftColumn = leftColumn.PadRight(columnWidth);
            }

            // Retrieve right column, trimming if needed
            var rightColumn = rightColumnLines[i];
            if (rightColumn.Length > columnWidth)
            {
                rightColumn = rightColumn[..(columnWidth - 3)] + "...";
            }

            sb.AppendLine($"{leftColumn}     {rightColumn}");
        }

        Console.Clear();
        Console.Write(sb.ToString());
    }

    private List<string> GetHeaderLines(int columnWidth)
    {
        var lines = new List<string>();

        var connected = $"Connected: {_trackerBase.AutoTracker?.IsConnected == true}";

        switch (_trackerBase.AutoTracker?.CurrentGame)
        {
            case Game.Zelda:
                lines.Add($"{connected} | {_trackerBase.AutoTracker.ZeldaState}");
                break;
            case Game.SM:
                lines.Add($"{connected} | {_trackerBase.AutoTracker.MetroidState}");
                break;
            default:
                lines.Add(connected);
                break;
        }

        lines.Add(new string('-', columnWidth * 2 + 5));

        return lines;
    }

    private IEnumerable<string> GetDungeonLines(int columnWidth)
    {
        var lines = new List<string> { "Dungeons", new('-', columnWidth) };

        var dungeons = _world.Dungeons
            .Select(x => GetDungeonDetails(x).PadRight(18))
            .ToList();

        var dungeonLine = "";
        foreach (var dungeon in dungeons)
        {
            if (dungeonLine == "")
            {
                dungeonLine = dungeon;
            }
            else if ($"{dungeonLine}{dungeon}".Length > columnWidth)
            {
                lines.Add(dungeonLine);
                dungeonLine = dungeon;
            }
            else
            {
                dungeonLine += dungeon;
            }
        }
        lines.Add(dungeonLine);

        return lines;
    }

    private IEnumerable<string> GetLocationLines(int columnWidth)
    {
        var lines = new List<string> { "Locations", new('-', columnWidth) };

        var locations = _worldService.Locations(unclearedOnly: true, outOfLogic: false, assumeKeys: true,
            sortByTopRegion: true, regionFilter: RegionFilter.None).ToList();

        var regionCounts = locations
            .GroupBy(x => x.Region)
            .OrderByDescending(x => x.Count())
            .ToDictionary(x => x.Key, x => x.Count());

        var locationNames = locations
            .OrderByDescending(x => x.Region == _lastRegion)
            .ThenByDescending(x => regionCounts[x.Region])
            .ThenBy(x => x.ToString())
            .Select(x => x.ToString());
        lines.AddRange(locationNames);

        return lines;
    }

    private IEnumerable<string> GetInventoryLines(int columnWidth)
    {
        var lines = new List<string> { "Inventory", new('-', columnWidth) };

        var itemNames = _world.AllItems.Where(x => x.State.TrackingState > 0)
            .DistinctBy(x => x.Type)
            .Where(x => !x.Type.IsInAnyCategory(ItemCategory.Junk, ItemCategory.Map, ItemCategory.Compass, ItemCategory.Keycard, ItemCategory.BigKey, ItemCategory.SmallKey) || x.Type is ItemType.Missile or ItemType.Super or ItemType.PowerBomb or ItemType.ETank)
            .OrderBy(x => x.Type.IsInCategory(ItemCategory.Metroid))
            .ThenBy(x => x.Name)
            .Select(x => x.Metadata.HasStages || x.Metadata.Multiple
                ? $"{x.Name} ({x.State.TrackingState})"
                : x.Name)
            .ToList();

        for (var i = 0; i < itemNames.Count; i += 2)
        {
            if (i < itemNames.Count - 1)
            {
                lines.Add(itemNames[i].PadRight(columnWidth/2) + itemNames[i+1]);
            }
            else
            {
                lines.Add(itemNames[i]);
            }
        }

        return lines;
    }

    private string GetDungeonDetails(IDungeon dungeon)
    {
        var state = dungeon.DungeonState.Cleared ? "\u2713" : "\u274c";
        var abbreviation = dungeon.Abbreviation;

        var reward = dungeon is IHasReward
            ? dungeon.MarkedReward switch
                {
                    RewardType.None => "??",
                    RewardType.CrystalBlue => "BC",
                    RewardType.CrystalRed => "RC",
                    RewardType.PendantBlue => "BP",
                    RewardType.PendantGreen => "GP",
                    RewardType.PendantRed => "RP",
                    _ => null
                }
            : null;

        var requirement = dungeon is INeedsMedallion
            ? dungeon.MarkedMedallion switch
            {
                ItemType.Quake => "Q",
                ItemType.Bombos => "B",
                ItemType.Ether => "E",
                _ => "?"
            }
            : null;

        var rewardRequirement = "";
        if (reward != null)
        {
            rewardRequirement = requirement == null ? $" ({reward})" : $" ({reward}/{requirement})";
        }

        return $"{state} {abbreviation}{rewardRequirement}: {dungeon.DungeonState.RemainingTreasure}";
    }

}
