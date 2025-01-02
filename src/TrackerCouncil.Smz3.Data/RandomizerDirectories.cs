using System;
using System.IO;
using System.Linq;

namespace TrackerCouncil.Smz3.Data;

public static class RandomizerDirectories
{
    public static string SolutionPath
    {
        get
        {
#if DEBUG
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return Path.Combine(directory!.FullName);
#else
            throw new InvalidOperationException("This method should only be called in debug mode.");
#endif
        }
    }

    public static string ConfigPath
    {
        get
        {
#if DEBUG
            var parentDir = new DirectoryInfo(SolutionPath).Parent;
            var configRepo = parentDir?.GetDirectories().FirstOrDefault(x => x.Name == "SMZ3CasConfigs");
            var basePath = Path.Combine(configRepo?.FullName ?? "", "Profiles");

            if (!Directory.Exists(basePath))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "Configs");
            }

            return basePath;
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "Configs");
#endif
        }
    }

    public static string SpritePath
    {
        get
        {
#if DEBUG
            var parentDir = new DirectoryInfo(SolutionPath).Parent;
            var spriteRepo = parentDir?.GetDirectories().FirstOrDefault(x => x.Name == "SMZ3CasSprites");
            var path = Path.Combine(spriteRepo?.FullName ?? "", "Sprites");

            if (!Directory.Exists(path) || path == "Sprites")
            {
                return Path.Combine(AppContext.BaseDirectory, "Sprites");
            }

            return path;
#else
            return Path.Combine(AppContext.BaseDirectory, "Sprites");
#endif
        }
    }

    public static bool DeleteSprites => SpritePath == Path.Combine(AppContext.BaseDirectory, "Sprites");

    public static string UserSpritePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SMZ3CasRandomizer", "Sprites");

#if DEBUG
    public static string SpriteHashYamlFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "sprite-hashes-debug.yml");
#else
    public static string SpriteHashYamlFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "sprite-hashes.yml");
#endif

    public static  string SpriteInitialJsonFilePath => Path.Combine(SpritePath, "sprites.json");

    public static string TrackerSpritePath
    {
        get
        {
#if DEBUG
            var parentDir = new DirectoryInfo(SolutionPath).Parent;
            var spriteRepo = parentDir?.GetDirectories().FirstOrDefault(x => x.Name == "TrackerSprites");

            if (spriteRepo?.Exists != true)
            {
                return Path.Combine(AppContext.BaseDirectory, "TrackerSprites");
            }

            return spriteRepo.FullName;
#else
            return Path.Combine(AppContext.BaseDirectory, "TrackerSprites");
#endif
        }
    }

#if DEBUG
    public static string TrackerSpriteHashYamlFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "tracker-sprite-hashes-debug.yml");
#else
    public static string TrackerSpriteHashYamlFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "tracker-sprite-hashes.yml");
#endif

    public static string TrackerSpriteInitialJsonFilePath => Path.Combine(TrackerSpritePath, "tracker-sprites.json");

    public static bool DeleteTrackerSprites => SpritePath == Path.Combine(AppContext.BaseDirectory, "Sprites");
}
