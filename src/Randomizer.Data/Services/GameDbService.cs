using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Services;

public class GameDbService : IGameDbService
{
    private readonly RandomizerContext _context;
    private readonly RandomizerOptions _options;

    public GameDbService(RandomizerContext context, OptionsFactory optionsFactory)
    {
        _context = context;
        _options = optionsFactory.Create();
    }

    public bool UpdateGeneratedRom(GeneratedRom rom, string? label = null)
    {
        var updated = false;

        if (label != null && label != rom.Label)
        {
            rom.Label = label;
            updated = true;
        }

        if (updated)
        {
            _context.SaveChanges();
        }

        return updated;
    }

    public IEnumerable<GeneratedRom> GetGeneratedRomsList()
    {
        return _context.GeneratedRoms
            .Include(x => x.MultiplayerGameDetails)
            .Include(x => x.TrackerState)
            .ThenInclude(x => x!.History)
            .Where(x => x.MultiplayerGameDetails == null);
    }

    public IEnumerable<MultiplayerGameDetails> GetMultiplayerGamesList()
    {
        return _context.MultiplayerGames
            .Include(x => x.GeneratedRom)
            .ThenInclude(x => x!.TrackerState);
    }

    public ICollection<TrackerHistoryEvent> GetGameHistory(GeneratedRom rom)
    {
        if (rom.TrackerState?.History.Count > 0)
        {
            return rom.TrackerState.History;
        }

        if (rom.TrackerState == null)
        {
            _context.Entry(rom).Reference<TrackerState>(x => x.TrackerState).Load();
            if (rom.TrackerState == null)
            {
                return new List<TrackerHistoryEvent>();
            }
        }

        _context.Entry(rom.TrackerState).Collection(x => x.History).Load();
        return rom.TrackerState.History;
    }

    public bool DeleteGeneratedRom(GeneratedRom rom, out string error)
    {
        // Try to delete the folder first
        try
        {
            var path = Path.GetDirectoryName(Path.Combine(_options.RomOutputPath, rom.RomPath));

            if (!string.IsNullOrEmpty(path))
            {
                var directory = new DirectoryInfo(path);
                directory.Delete(true);
            }
        }
        catch (Exception ex)
        {
            if (ex is not DirectoryNotFoundException)
            {
                error = "There was an error in trying to delete the rom directory. Verify the rom is not open in your emulator.";
                return false;
            }
        }

        // Delete the tracker info if it is available
        if (rom.TrackerState != null)
        {
            _context.Entry(rom.TrackerState).Collection(x => x.ItemStates).Load();
            _context.Entry(rom.TrackerState).Collection(x => x.LocationStates).Load();
            _context.Entry(rom.TrackerState).Collection(x => x.RegionStates).Load();
            _context.Entry(rom.TrackerState).Collection(x => x.DungeonStates).Load();
            _context.Entry(rom.TrackerState).Collection(x => x.MarkedLocations).Load();
            _context.Entry(rom.TrackerState).Collection(x => x.BossStates).Load();
            _context.Entry(rom.TrackerState).Collection(x => x.History).Load();

            _context.TrackerStates.Remove(rom.TrackerState);
        }

        // Remove the rom itself from the db and save the db
        _context.GeneratedRoms.Remove(rom);
        _context.SaveChanges();

        error = "";
        return true;
    }

    public bool DeleteMultiplayerGame(MultiplayerGameDetails details, out string error)
    {
        // If there's a rom, try to delete it first and don't continue
        // if it wasn't deleted
        if (details.GeneratedRom != null)
        {
            if (!DeleteGeneratedRom(details.GeneratedRom, out error))
            {
                return false;
            }
        }
        else
        {
            var rom = _context.GeneratedRoms
                .Where(x => x.MultiplayerGameDetailsId == details.Id)
                .Include(x => x.MultiplayerGameDetails)
                .Include(x => x.TrackerState)
                .ThenInclude(x => x!.History)
                .FirstOrDefault();

            if (rom != null && !DeleteGeneratedRom(rom, out error))
            {
                return false;
            }
        }

        _context.MultiplayerGames.Remove(details);
        _context.SaveChanges();

        error = "";
        return true;
    }

}
