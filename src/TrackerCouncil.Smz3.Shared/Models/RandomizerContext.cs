using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618

namespace TrackerCouncil.Smz3.Shared.Models;

public sealed class RandomizerContext : DbContext
{

    public RandomizerContext() : base()
    {
        Database.Migrate();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"FileName={DbPath}", option =>
        {
            option.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
        });
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeneratedRom>().HasOne(x => x.TrackerState);
        modelBuilder.Entity<GeneratedRom>().HasKey(x => x.Id);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.ItemStates).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.LocationStates).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.BossStates).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.TreasureStates).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.RewardStates).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.PrerequisiteStates).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.History).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.Hints).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);

#pragma warning disable CS0618 // Type or member is obsolete
        modelBuilder.Entity<TrackerState>().HasMany(x => x.RegionStates).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.DungeonStates).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrackerState>().HasMany(x => x.MarkedLocations).WithOne(x => x.TrackerState!).OnDelete(DeleteBehavior.Cascade);
#pragma warning restore CS0618 // Type or member is obsolete

        modelBuilder.Entity<MultiplayerGameDetails>().HasOne(x => x.GeneratedRom);

        base.OnModelCreating(modelBuilder);

    }

    public DbSet<GeneratedRom> GeneratedRoms { get; set; }
    public DbSet<TrackerState> TrackerStates { get; set; }
    public DbSet<TrackerItemState> TrackerItemStates { get; set; }
    public DbSet<TrackerLocationState> TrackerLocationStates { get; set; }
    public DbSet<TrackerBossState> TrackerBossStates { get; set; }
    public DbSet<TrackerTreasureState> TrackerTreasureStates { get; set; }
    public DbSet<TrackerRewardState> TrackerRewardStates { get; set; }
    public DbSet<TrackerPrerequisiteState> TrackerPrerequisiteStates { get; set; }
    public DbSet<TrackerHistoryEvent> TrackerHistoryEvents { get; set; }
    public DbSet<TrackerHintState> TrackerHintStates { get; set; }
    public DbSet<TrackerRegionState> TrackerRegionStates { get; set; }
    public DbSet<TrackerDungeonState> TrackerDungeonStates { get; set; }
    public DbSet<TrackerMarkedLocation> TrackerMarkedLocations { get; set; }
    public DbSet<MultiplayerGameDetails> MultiplayerGames { get; set; }

#if DEBUG
    private static string DbPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "smz3-debug.db");
#else
    private static string DbPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "smz3.db");
#endif

}
