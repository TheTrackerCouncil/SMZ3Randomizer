using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Randomizer.Data.Options;
using Randomizer.SMZ3.FileData;

namespace Randomizer.Tools;

public class RomGenerator
{
    private const string SmRomPath = @"D:\Games\SMZ3\sm.sfc";
    private const string Z3RomPath = @"D:\Games\SMZ3\z3.sfc";
    private const string OutputPath = @"D:\Games\SMZ3\TestRom";
    private const string MsuPath = @"D:\Games\SMZ3\SMZ3MSUs\RetroPC\RPC-LTTP-MSU.msu";
    private const string RomName = "test-rom";

    private static readonly Dictionary<string, string> s_ipsPatches = new()
    {
        { @"..\..\..\..\Randomizer.App\Patches\zsm.ips", "" },
        { @"..\..\..\..\Randomizer.App\Patches\spinjumprestart.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\nerfed_charge.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\refill_before_save.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\fast_doors.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\elevators_speed.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\AimAnyButton.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\rando_speed.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\noflashing.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\disable_screen_shake.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\Celeste.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\EasierWJ.ips", "SM" },
    };

    public static string GenerateRom(string[] args)
    {
        var openRom = args.Any(x => "openrom".Equals(x, StringComparison.OrdinalIgnoreCase));
        var copyMsu = args.Any(x => "copymsu".Equals(x, StringComparison.OrdinalIgnoreCase));
        var copyPatches = args.Any(x =>  "copypatches".Equals(x, StringComparison.OrdinalIgnoreCase));
        var applyPatches = args.Any(x =>  "applypatches".Equals(x, StringComparison.OrdinalIgnoreCase));
        var batFile = new FileInfo(@"..\..\..\..\..\alttp_sm_combo_randomizer_rom\build.bat");
        var romPath = Path.Combine(OutputPath, $"{RomName}.sfc");

        // Run the patch build.bat file
        var process = Process.Start(new ProcessStartInfo()
        {
            FileName = batFile.FullName,
            WorkingDirectory = batFile.Directory?.FullName,
            UseShellExecute = true
        });
        if (process == null) return "";
        process.WaitForExit();
        process.Close();

        // Verify that the main IPS file was created
        var info = new FileInfo(s_ipsPatches.Keys.First(x => x.Contains("zsm.ips")));
        var length = info.Length;
        if (length < 1000) return "";

        // Copy the build.bat output patches to the SMZ3 patch folder
        if (copyPatches)
        {
            var resources = new DirectoryInfo(@"..\..\..\..\..\alttp_sm_combo_randomizer_rom\build");
            foreach (var file in resources.EnumerateFiles().Where(x => x.Extension.Contains("ips")))
            {
                file.CopyTo(@"..\..\..\..\Randomizer.App\Patches\"+file.Name, true);
            }
        }

        using var smRom = File.OpenRead(SmRomPath);
        using var z3Rom = File.OpenRead(Z3RomPath);
        var rom = Rom.CombineSMZ3Rom(smRom, z3Rom);

        // Apply all IPS patches
        foreach (var patch in s_ipsPatches)
        {
            if (!applyPatches && !patch.Key.EndsWith("zsm.ips")) continue;

            using var ips = File.OpenRead(patch.Key);

            switch (patch.Value)
            {
                case "SM":
                    Rom.ApplySuperMetroidIps(rom, ips);
                    break;
                default:
                    Rom.ApplyIps(rom, ips);
                    break;
            }

        }

        // Apply a sprite
        if (applyPatches)
        {
            var sprite = new Sprite("", "", @"..\..\..\..\Randomizer.App\Sprites\Samus\rash.rdc", SpriteType.Samus);
            sprite.ApplyTo(rom);
        }

        Directory.CreateDirectory(OutputPath);

        // Apply additional patches
        if (applyPatches)
        {
            var patches = new List<(int offset, byte[] bytes)> { (Patcher.Snes(0x1C4800+0x2), Patcher.UshortBytes(0xFF)) };
            var patchDictionary = patches.ToDictionary(x => x.offset, x => x.bytes);
            Rom.ApplySeed(rom, patchDictionary);

            if (File.Exists(romPath)) File.Delete(romPath);
            Rom.UpdateChecksum(rom);
        }

        File.WriteAllBytes(romPath, rom);

        if (copyMsu)
        {
            foreach (var file in Directory.GetFiles(OutputPath))
            {
                if (file.EndsWith(".pcm") || file.EndsWith(".msu"))
                {
                    File.Delete(file);
                }
            }

            if (!string.IsNullOrEmpty(MsuPath))
            {
                File.Copy(MsuPath, Path.Combine(OutputPath, $"{RomName}.msu"));
                for (var i = 1; i < 200; i++)
                {
                    var pcmPath = MsuPath.Replace(".msu", $"-{i}.pcm");
                    if (File.Exists(pcmPath))
                        NativeMethods.CreateHardLink(Path.Combine(OutputPath, $"{RomName}-{i}.pcm"), pcmPath, IntPtr.Zero);
                }
            }
        }

        if (openRom)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = romPath,
                UseShellExecute = true
            });
        }

        return romPath;
    }
}
