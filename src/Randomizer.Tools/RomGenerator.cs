using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Randomizer.App.Patches;
using Randomizer.SMZ3.FileData;

namespace Randomizer.Tools;

public class RomGenerator
{
    private const string SmRomPath = @"D:\Games\SMZ3\Super Metroid (Japan, USA) (En,Ja).sfc";
    private const string Z3RomPath = @"D:\Games\SMZ3\Zelda no Densetsu - Kamigami no Triforce (Japan).sfc";
    private const string IpsPath = @"D:\Source\SMZ3Randomizer\alttp_sm_combo_randomizer_rom\build\zsm.ips";
    private const string OutputPath = @"D:\Games\SMZ3\TestRom";
    private const string PatchBatFile = @"D:\Source\SMZ3Randomizer\alttp_sm_combo_randomizer_rom\build.bat";
    private const string MsuPath = @"E:\SMZ3\MSUs\RetroPC\RPC-LTTP-MSU.msu";
    private const string RomName = "test-rom";



    public static string GenerateRom(string[] args)
    {
        var openRom = args.Any(x => "openrom".Equals(x, StringComparison.OrdinalIgnoreCase));
        var copyMsu = args.Any(x => "copymsu".Equals(x, StringComparison.OrdinalIgnoreCase));

        var process = Process.Start(new ProcessStartInfo()
        {
            FileName = PatchBatFile,
            WorkingDirectory = Path.GetDirectoryName(PatchBatFile),
            UseShellExecute = true
        });
        if (process == null) return "";
        process.WaitForExit();
        process.Close();

        var info = new FileInfo(IpsPath);
        var length = info.Length;
        if (length < 1000) return "";

        using var smRom = File.OpenRead(SmRomPath);
        using var z3Rom = File.OpenRead(Z3RomPath);
        using var ips = File.OpenRead(IpsPath);
        var rom = Rom.CombineSMZ3Rom(smRom, z3Rom);
        Rom.ApplyIps(rom, ips);
        Directory.CreateDirectory(OutputPath);

        var patches = new List<(int offset, byte[] bytes)>();
        //patches.Add((Patcher.Snes(0x1C4800+0x2), Patcher.UshortBytes(0xFF)));
        var patchDictionary = patches.ToDictionary(x => x.offset, x => x.bytes);;
        Rom.ApplySeed(rom, patchDictionary);

        var romPath = Path.Combine(OutputPath, $"{RomName}.sfc");
        if (File.Exists(romPath)) File.Delete(romPath);
        Rom.UpdateChecksum(rom);
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
