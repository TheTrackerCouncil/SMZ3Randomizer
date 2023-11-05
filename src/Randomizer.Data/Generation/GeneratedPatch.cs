﻿namespace Randomizer.SMZ3.FileData.Patches;

public class GeneratedPatch
{
    public GeneratedPatch(int offset, byte[] data)
    {
        Offset = offset;
        Data = data;
    }

    public int Offset { get; set; }
    public byte[] Data { get; set; }
}
