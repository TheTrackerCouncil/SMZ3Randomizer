using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Randomizer.Shared.Models;

namespace Randomizer.Shared.Models {

    public class RandomizerContext : DbContext {

        public RandomizerContext() : base()
        {
            Database.EnsureCreatedAsync();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Environment.ExpandEnvironmentVariables("%LocalAppData%\\SMZ3CasRandomizer\\smz3.db");
            if (Debugger.IsAttached)
            {
                dbPath = "smz3.db";
            }

            optionsBuilder.UseSqlite($"FileName={dbPath}", option =>
            {
                option.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeneratedRom>().HasOne(x => x.TrackerState);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.ItemStates).WithOne(x => x.TrackerState).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.LocationStates).WithOne(x => x.TrackerState).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.RegionStates).WithOne(x => x.TrackerState).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.DungeonStates).WithOne(x => x.TrackerState).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.MarkedLocations).WithOne(x => x.TrackerState).OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);

        }

        public DbSet<GeneratedRom> GeneratedRoms { get; set; }
        public DbSet<TrackerState> TrackerStates { get; set; }
        public DbSet<TrackerItemState> TrackerItemStates { get; set; }
        public DbSet<TrackerLocationState> TrackerLocationStates { get; set; }
        public DbSet<TrackerRegionState> TrackerRegionStates { get; set; }
        public DbSet<TrackerDungeonState> TrackerDungeonStates { get; set; }
        public DbSet<TrackerMarkedLocation> TrackerMarkedLocations { get; set; }

    }

}
