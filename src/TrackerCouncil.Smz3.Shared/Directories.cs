using System;
using System.IO;
using System.Linq;

namespace TrackerCouncil.Smz3.Shared;

public static class Directories
{
    public static string AppDataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer");
    public static string LogFolder => Path.Combine(AppDataFolder, "Logs");
#if DEBUG
    public static string LogPath => Path.Combine(LogFolder, "smz3-cas-debug-.log");
#else
    public static string LogPath => Path.Combine(LogFolder, "smz3-cas-.log");
#endif

    public static string DefaultDataPath = Path.Combine(AppDataFolder, "DefaultData");

    public static string DefaultSeedPath = Path.Combine(AppDataFolder, "Seeds");

    public static string DefaultAutoTrackerScriptsPath = Path.Combine(AppDataFolder, "AutoTrackerScripts");

    public static string UserDataPath = Path.Combine(AppDataFolder, "CustomData");
    public static string UserPresetsPath => Path.Combine(UserDataPath, "Presets");

    #region Configs

    public static string DefaultDataConfigPath = Path.Combine(DefaultDataPath, "Configs");

    public static string ConfigPath
    {
        get
        {
#if DEBUG
            return GetGitRepoDirectory("SMZ3CasConfigs", "Profiles") ?? DefaultDataConfigPath;
#else
            return DefaultDataConfigPath;
#endif
        }
    }

    public static string UserConfigPath => Path.Combine(UserDataPath, "Configs");

    #endregion

    #region Sprites
    public static string DefaultDataSpritePath = Path.Combine(DefaultDataPath, "Sprites");

    public static string SpritePath
    {
        get
        {
#if DEBUG
            return GetGitRepoDirectory("SMZ3CasSprites", "Sprites") ?? DefaultDataSpritePath;
#else
            return DefaultDataSpritePath;
#endif
        }
    }

    public static bool DeleteSprites => SpritePath == DefaultDataSpritePath;

    public static string UserSpritePath => Path.Combine(UserDataPath, "Sprites");

#if DEBUG
    public static string SpriteHashYamlFilePath => Path.Combine(AppDataFolder, "sprite-hashes-debug.yml");
#else
    public static string SpriteHashYamlFilePath => Path.Combine(AppDataFolder, "sprite-hashes.yml");
#endif

    public static  string SpriteInitialJsonFilePath => Path.Combine(SpritePath, "sprites.json");

    #endregion

    #region Tracker Sprites
    public static string DefaultDataTrackerSpritePath = Path.Combine(DefaultDataPath, "TrackerSprites");

    public static string TrackerSpritePath
    {
        get
        {
#if DEBUG
            return GetGitRepoDirectory("TrackerSprites") ?? DefaultDataTrackerSpritePath;
#else
            return DefaultDataTrackerSpritePath;
#endif
        }
    }

    public static string UserTrackerSpritePath => Path.Combine(UserDataPath, "TrackerSprites");

#if DEBUG
    public static string TrackerSpriteHashYamlFilePath => Path.Combine(AppDataFolder, "tracker-sprite-hashes-debug.yml");
#else
    public static string TrackerSpriteHashYamlFilePath => Path.Combine(AppDataFolder, "tracker-sprite-hashes.yml");
#endif

    public static string TrackerSpriteInitialJsonFilePath => Path.Combine(TrackerSpritePath, "tracker-sprites.json");

    public static bool DeleteTrackerSprites => TrackerSpritePath == DefaultDataTrackerSpritePath;

    #endregion

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

    private static string? GetGitRepoDirectory(string directoryName, string? subdirectoryName = null)
    {
        var parentDir = new DirectoryInfo(SolutionPath).Parent;
        var baseDirectory = parentDir?.GetDirectories().FirstOrDefault(x => x.Name == directoryName)?.FullName;

        if (string.IsNullOrEmpty(baseDirectory))
        {
            return null;
        }

        var path = string.IsNullOrEmpty(subdirectoryName) ? baseDirectory : Path.Combine(baseDirectory, subdirectoryName);

        if (Directory.Exists(path))
        {
            return path;
        }

        return null;
    }
}
