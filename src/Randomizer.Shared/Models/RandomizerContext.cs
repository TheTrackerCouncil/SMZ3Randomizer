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
            modelBuilder.Entity<GeneratedRom>().ToTable("GeneratedRom", "smz3cas");
            modelBuilder.Entity<TrackerState>().ToTable("TrackerStates", "smz3cas");
            modelBuilder.Entity<TrackerItemState>().ToTable("TrackerItemStates", "smz3cas");
            modelBuilder.Entity<TrackerLocationState>().ToTable("TrackerLocationStates", "smz3cas");
            modelBuilder.Entity<TrackerRegionState>().ToTable("TrackerRegionStates", "smz3cas");
            modelBuilder.Entity<TrackerDungeonState>().ToTable("TrackerDungeonStates", "smz3cas");
            modelBuilder.Entity<TrackerMarkedLocation>().ToTable("TrackerMarkedLocations", "smz3cas");
        }

        public DbSet<GeneratedRom> Seeds { get; set; }
        public DbSet<TrackerState> TrackerStates { get; set; }
        public DbSet<TrackerItemState> TrackerItemStates { get; set; }
        public DbSet<TrackerLocationState> TrackerLocationStates { get; set; }
        public DbSet<TrackerRegionState> TrackerRegionState { get; set; }
        public DbSet<TrackerDungeonState> TrackerDungeonStates { get; set; }
        public DbSet<TrackerMarkedLocation> TrackerMarkedLocations { get; set; }

    }

}
