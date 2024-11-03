using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrackerCouncil.Smz3.Shared.Multiplayer;

#pragma warning disable CS8618

namespace TrackerCouncil.Smz3.Multiplayer.Server;

public sealed class MultiplayerDbContext : DbContext
{
    public static bool IsSetup { get; private set; }
    private readonly string? _sqliteConnectionString;

    public MultiplayerDbContext(IOptions<SMZ3ServerSettings> options)
    {
        _sqliteConnectionString = $"FileName={options.Value.SQLiteFilePath}";
        if (string.IsNullOrEmpty(_sqliteConnectionString)) return;
        Database.Migrate();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (string.IsNullOrEmpty(_sqliteConnectionString)) return;
        optionsBuilder.UseSqlite(_sqliteConnectionString, option =>
        {
            option.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
        });
        base.OnConfiguring(optionsBuilder);
        IsSetup = true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MultiplayerGameState>().HasMany(x => x.Players).WithOne(x => x.Game).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MultiplayerPlayerState>().HasMany(x => x.Locations).WithOne(x => x.Player)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MultiplayerPlayerState>().HasMany(x => x.Items).WithOne(x => x.Player)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MultiplayerPlayerState>().HasMany(x => x.Bosses).WithOne(x => x.Player)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<MultiplayerGameState> MultiplayerGameStates { get; init; }
    public DbSet<MultiplayerPlayerState> MultiplayerPlayerStates { get; init; }
    public DbSet<MultiplayerLocationState> MultiplayerLocationStates { get; init; }
    public DbSet<MultiplayerItemState> MultiplayerItemStates { get; init; }
    public DbSet<MultiplayerBossState> MultiplayerBossStates { get; init; }

}
