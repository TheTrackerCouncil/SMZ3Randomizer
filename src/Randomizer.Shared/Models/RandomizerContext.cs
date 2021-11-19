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
            optionsBuilder.UseSqlite("FileName=sqlitedb", option =>
            {
                option.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeneratedRom>().HasOne(x => x.TrackerState);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.ItemStates);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.LocationStates);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.RegionStates);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.DungeonStates);
            modelBuilder.Entity<TrackerState>().HasMany(x => x.MarkedLocations);

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
