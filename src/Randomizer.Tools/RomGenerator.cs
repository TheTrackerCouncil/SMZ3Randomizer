using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Randomizer.App;
using Randomizer.Data.Options;
using Randomizer.SMZ3.FileData;
using Randomizer.SMZ3.FileData.Patches;

namespace Randomizer.Tools;

public class RomGenerator
{
    private const string SmRomPath = @"D:\Games\SMZ3\sm.sfc";
    private const string Z3RomPath = @"D:\Games\SMZ3\z3.sfc";
    private const string OutputPath = @"D:\Games\SMZ3\TestRom";
    private const string MsuPath = @"D:\Games\SMZ3\SMZ3MSUs\Yoshi's Scrambled Worlds - Copy\Yoshi_SMZ3.msu";
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
        { @"..\..\..\..\Randomizer.App\Patches\UnifiedAim.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\AutoRun.ips", "SM" },
        { @"..\..\..\..\Randomizer.App\Patches\HoldFire.ips", "SM" },
    };

    private static readonly List<RomPatch> s_romPatches = new() { new GoalsPatch(), new InfiniteSpaceJumpPatch(), new MenuSpeedPatch(), new MetroidControlsPatch() };

    private static readonly Config s_config = new()
    {
        OpenPyramid = true,
        MenuSpeed = MenuSpeed.Fast,
        MetroidControls = new MetroidControlOptions()
        {
            Shoot = MetroidButton.Y,
            Jump = MetroidButton.B,
            Dash = MetroidButton.X,
            ItemSelect = MetroidButton.Select,
            ItemCancel = MetroidButton.R,
            AimUp = MetroidButton.L,
            AimDown = MetroidButton.A,
            RunButtonBehavior = RunButtonBehavior.AutoRun,
            ItemCancelBehavior = ItemCancelBehavior.HoldSupersOnly,
            AimButtonBehavior = AimButtonBehavior.UnifiedAim
        }
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
            var patches = new List<(int offset, byte[] bytes)> { (Patcher.Snes(0x40008B), Patcher.UshortBytes(0x01)) };
            var patchDictionary = patches.ToDictionary(x => x.offset, x => x.bytes);
            foreach (var patcher in s_romPatches)
            {
                foreach (var (offset, bytes) in patcher.GetChanges(s_config))
                {
                    patchDictionary[offset] = bytes;
                }
            }
            Rom.ApplySeed(rom, patchDictionary);

            if (File.Exists(romPath)) File.Delete(romPath);
            Rom.UpdateChecksum(rom);
        }

        File.WriteAllBytes(romPath, rom);

        if (copyMsu)
        {
            var msuGenerator = new MsuGeneratorService();
            if (!msuGenerator.EnableMsu1Support(MsuPath, romPath, out var error))
            {
                throw new Exception(error);
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
