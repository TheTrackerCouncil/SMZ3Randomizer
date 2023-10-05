using System;
using System.IO;
using System.Security.Cryptography;

namespace Randomizer.Data.Services;

public class SourceRomValidationService
{
    public bool ValidateZeldaRom(string? path)
    {
        return ValidateRom(path, "03a63945398191337e896e5771f77173");
    }

    public bool ValidateMetroidRom(string? path)
    {
        return ValidateRom(path, "21f3e98df4780ee1c667b84e57d88675");
    }

    private bool ValidateRom(string? path, string expectedHash)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (!File.Exists(path))
        {
            return false;
        }

        using var md5 = MD5.Create();
        using var stream = File.OpenRead(path);
        var hash = md5.ComputeHash(stream);
        var hashString = BitConverter.ToString(hash).Replace("-", "");
        return expectedHash.Equals(hashString, StringComparison.OrdinalIgnoreCase);
    }
}
