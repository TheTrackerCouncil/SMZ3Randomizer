using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.Services;

public class GameDbService(RandomizerContext context, OptionsFactory optionsFactory) : IGameDbService
{
    private readonly RandomizerOptions _options = optionsFactory.Create();

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
            context.SaveChanges();
        }

        return updated;
    }

    public IEnumerable<GeneratedRom> GetGeneratedRomsList()
    {
        return context.GeneratedRoms
            .Include(x => x.MultiplayerGameDetails)
            .Include(x => x.TrackerState)
            .ThenInclude(x => x!.History)
            .Where(x => x.MultiplayerGameDetails == null);
    }

    public IEnumerable<MultiplayerGameDetails> GetMultiplayerGamesList()
    {
        return context.MultiplayerGames
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
            context.Entry(rom).Reference<TrackerState>(x => x.TrackerState).Load();
            if (rom.TrackerState == null)
            {
                return new List<TrackerHistoryEvent>();
            }
        }

        context.Entry(rom.TrackerState).Collection(x => x.History).Load();
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
            context.Entry(rom.TrackerState).Collection(x => x.ItemStates).Load();
            context.Entry(rom.TrackerState).Collection(x => x.LocationStates).Load();
            context.Entry(rom.TrackerState).Collection(x => x.BossStates).Load();
            context.Entry(rom.TrackerState).Collection(x => x.RewardStates).Load();
            context.Entry(rom.TrackerState).Collection(x => x.PrerequisiteStates).Load();
            context.Entry(rom.TrackerState).Collection(x => x.TreasureStates).Load();
            context.Entry(rom.TrackerState).Collection(x => x.Hints).Load();
            context.Entry(rom.TrackerState).Collection(x => x.History).Load();

#pragma warning disable CS0618 // Type or member is obsolete
            context.Entry(rom.TrackerState).Collection(x => x.RegionStates).Load();
            context.Entry(rom.TrackerState).Collection(x => x.DungeonStates).Load();
            context.Entry(rom.TrackerState).Collection(x => x.MarkedLocations).Load();
#pragma warning restore CS0618 // Type or member is obsolete

            context.TrackerStates.Remove(rom.TrackerState);
        }

        // Remove the rom itself from the db and save the db
        context.GeneratedRoms.Remove(rom);
        context.SaveChanges();

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
            var rom = context.GeneratedRoms
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

        context.MultiplayerGames.Remove(details);
        context.SaveChanges();

        error = "";
        return true;
    }

}
