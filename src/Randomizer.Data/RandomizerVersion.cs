using System;

namespace Randomizer.Data;

public class RandomizerVersion
{
    // Update this whenever we have breaking logic changes which might affect
    // old seeds. Only use the major version number.
    public static Version Version => new(6, 0);

    public static string VersionString => Version.ToString();

    public static int MajorVersion => Version.Major;
}
