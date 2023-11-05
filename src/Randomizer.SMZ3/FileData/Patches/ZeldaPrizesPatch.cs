﻿using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Options;
using Randomizer.Shared;

namespace Randomizer.SMZ3.FileData.Patches;

[Order(-7)]
public class ZeldaPrizesPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        const int prizePackItems = 56;
        const int treePullItems = 3;

        var prizes = DropPrizes.GetPool(data.Config.ZeldaDrops, data.Random).Cast<byte>();

        /* prize pack drop order */
        (var patch, prizes) = GetDropPatch(0x6FA78, prizes, data.PlandoConfig.ZeldaPrizes.EnemyDrops, prizePackItems);
        yield return patch;

        /* tree pull prizes */
        (patch, prizes) = GetDropPatch(0x1DFBD4, prizes, data.PlandoConfig.ZeldaPrizes.TreePulls, treePullItems);
        yield return patch;

        /* crab prizes */
        (patch, prizes) = GetDropPatch(0x6A9C8, prizes, data.PlandoConfig.ZeldaPrizes.CrabBaseDrop);
        yield return patch;
        (patch, prizes) = GetDropPatch(0x6A9C4, prizes, data.PlandoConfig.ZeldaPrizes.CrabEightDrop);
        yield return patch;

        /* stun prize */
        (patch, prizes) = GetDropPatch(0x6F993, prizes, data.PlandoConfig.ZeldaPrizes.StunPrize);
        yield return patch;

        /* fish prize */
        (patch, _) = GetDropPatch(0x1D82CC, prizes, data.PlandoConfig.ZeldaPrizes.FishPrize);
        yield return patch;

        foreach (var enemyDistributionPatch in EnemyPrizePackDistribution(data))
        {
            yield return enemyDistributionPatch;
        }

        /* Pack drop chance */
        /* Normal difficulty is 50%. 0 => 100%, 1 => 50%, 3 => 25% */
        const int nrPacks = 7;
        const byte probability = 1;
        yield return new GeneratedPatch(Snes(0x6FA62), Enumerable.Repeat(probability, nrPacks).ToArray());
    }

    private (GeneratedPatch patch, IEnumerable<byte> prizes) GetDropPatch(int location, IEnumerable<byte> prizes,
        IEnumerable<DropPrize>? dropOverride, int count)
    {
        if (dropOverride?.Any() == true)
        {
            var bytes = dropOverride.Cast<byte>().ToList();
            if (bytes.Count < count)
            {
                (var additionalBytes, prizes) = prizes.SplitOff(count - bytes.Count);
                bytes.AddRange(additionalBytes);
            }
            return (new GeneratedPatch(Snes(location), bytes.ToArray()), prizes);
        }
        else
        {
            (var bytes, prizes) = prizes.SplitOff(count);
            return (new GeneratedPatch(Snes(location), bytes.ToArray()), prizes);
        }
    }

    private (GeneratedPatch patch, IEnumerable<byte> prizes) GetDropPatch(int location, IEnumerable<byte> prizes,
        DropPrize? dropOverride)
    {
        if (dropOverride == null)
        {
            (var bytes, prizes) = prizes.SplitOff(1);
            return (new GeneratedPatch(Snes(location), bytes.ToArray()), prizes);
        }
        else
        {
            return (new GeneratedPatch(Snes(location), new[] { (byte)dropOverride }), prizes);
        }
    }

    private IEnumerable<GeneratedPatch> EnemyPrizePackDistribution(GetPatchesRequest data)
    {
        var (prizePacks, duplicatePacks) = EnemyPrizePacks();

        var n = prizePacks.Sum(x => x.bytes.Length);
        var randomization = PrizePackRandomization(data, n, 1);

        var patches = prizePacks.Select(x =>
        {
            (var packs, randomization) = randomization.SplitOff(x.bytes.Length);
            return (x.offset, bytes: x.bytes.Zip(packs, (b, p) => (byte)(b | p)).ToArray());
        }).ToList();

        var duplicates =
            from d in duplicatePacks
            from p in patches
            where p.offset == d.src
            select (d.dest, p.bytes);
        patches.AddRange(duplicates.ToList());

        return patches.Select(x => new GeneratedPatch(Snes(x.offset), x.bytes));
    }

    /* Guarantees at least s of each prize pack, over a total of n packs.
     * In each iteration, from the product n * m, use the guaranteed number
     * at k, where k is the "row" (integer division by m), when k falls
     * within the list boundary. Otherwise use the "column" (modulo by m)
     * as the random element.
     */

    private IEnumerable<byte> PrizePackRandomization(GetPatchesRequest data, int n, int s)
    {
        const int m = 7;
        var g = Enumerable.Repeat(Enumerable.Range(0, m), s)
            .SelectMany(x => x)
            .ToList();

        IEnumerable<int> Randomization(int x)
        {
            x = m * x;
            while (x > 0)
            {
                var r = data.Random.Next(x);
                var k = r / m;
                yield return k < g.Count ? g[k] : r % m;
                if (k < g.Count) g.RemoveAt(k);
                x -= m;
            }
        }

        return Randomization(n).Select(x => (byte)(x + 1)).ToList();
    }

    /* Todo: Deadrock turns into $8F Blob when powdered, but those "onion blobs" always drop prize pack 1. */

    private (IList<(int offset, byte[] bytes)>, IList<(int src, int dest)>) EnemyPrizePacks()
    {
        const int offset = 0xDB632;
        var patches = new[] {
            /* sprite_prep */
            (0x6888D, new byte[] { 0x00 }), // Keese DW
            (0x688A8, new byte[] { 0x00 }), // Rope
            (0x68967, new byte[] { 0x00, 0x00 }), // Crow/Dacto
            (0x69125, new byte[] { 0x00, 0x00 }), // Red/Blue Hardhat Bettle
            /* sprite properties */
            (offset+0x01, new byte[] { 0x90 }), // Vulture
            (offset+0x08, new byte[] { 0x00 }), // Octorok (One Way)
            (offset+0x0A, new byte[] { 0x00 }), // Octorok (Four Way)
            (offset+0x0D, new byte[] { 0x80, 0x90 }), // Buzzblob, Snapdragon
            (offset+0x11, new byte[] { 0x90, 0x90, 0x00 }), // Hinox, Moblin, Mini Helmasaur
            (offset+0x18, new byte[] { 0x90, 0x90 }), // Mini Moldorm, Poe/Hyu
            (offset+0x20, new byte[] { 0x00 }), // Sluggula
            (offset+0x22, new byte[] { 0x80, 0x00, 0x00 }), // Ropa, Red Bari, Blue Bari
            // Blue Soldier/Tarus, Green Soldier, Red Spear Soldier Blue
            // Assault Soldier, Red Assault Spear Soldier/Tarus Blue Archer,
            // Green Archer Red Javelin Soldier, Red Bush Javelin Soldier
            // Red Bomb Soldiers, Green Soldier Recruits, Geldman, Toppo
            (offset+0x41, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x10, 0x90, 0x90, 0x80 }),
            (offset+0x4F, new byte[] { 0x80 }), // Popo 2
            (offset+0x51, new byte[] { 0x80 }), // Armos
            (offset+0x55, new byte[] { 0x00, 0x00 }), // Ku, Zora
            (offset+0x58, new byte[] { 0x90 }), // Crab
            (offset+0x64, new byte[] { 0x80 }), // Devalant (Shooter)
            (offset+0x6A, new byte[] { 0x90, 0x90 }), // Ball N' Chain Trooper, Cannon Soldier
            (offset+0x6D, new byte[] { 0x80, 0x80 }), // Rat/Buzz, (Stal)Rope
            (offset+0x71, new byte[] { 0x80 }), // Leever
            (offset+0x7C, new byte[] { 0x90 }), // Initially Floating Stal
            (offset+0x81, new byte[] { 0xC0 }), // Hover
            // Green Eyegore/Mimic, Red Eyegore/Mimic Detached Stalfos Body,
            // Kodongo
            (offset+0x83, new byte[] { 0x10, 0x10, 0x10, 0x00 }),
            (offset+0x8B, new byte[] { 0x10 }), // Gibdo
            (offset+0x8E, new byte[] { 0x00, 0x00 }), // Terrorpin, Blob
            (offset+0x91, new byte[] { 0x10 }), // Stalfos Knight
            (offset+0x99, new byte[] { 0x10 }), // Pengator
            (offset+0x9B, new byte[] { 0x10 }), // Wizzrobe
            // Blue Zazak, Red Zazak, Stalfos Green Zirro, Blue Zirro, Pikit
            (offset+0xA5, new byte[] { 0x10, 0x10, 0x10, 0x80, 0x80, 0x80 }),
            (offset+0xC7, new byte[] { 0x10 }), // Hokku-Bokku
            (offset+0xC9, new byte[] { 0x10 }), // Tektite
            (offset+0xD0, new byte[] { 0x10 }), // Lynel
            (offset+0xD3, new byte[] { 0x00 }), // Stal
        };
        var duplicates = new[] {
            /* Popo2 -> Popo. Popo is not used in vanilla Z3, but we duplicate from Popo2 just to be sure */
            (offset + 0x4F, offset + 0x4E),
        };
        return (patches, duplicates);
    }
}
