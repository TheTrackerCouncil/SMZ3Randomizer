using System;
using System.IO;

namespace Randomizer.CrossPlatform;

public class Directories
{
    public static string AppDataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer");
    public static string LogFolder => Path.Combine(AppDataFolder, "Logs");
#if DEBUG
    public static string LogPath => Path.Combine(LogFolder, "smz3-cas-debug-.log");
#else
    public static string LogPath => Path.Combine(LogFolder, "smz3-cas-.log");
#endif
}
