namespace Randomizer.Shared.Enums;

/// <summary>
/// The type of memory
/// </summary>
public enum MemoryDomain
{
    /// <summary>
    /// SNES Memory
    /// </summary>
    WRAM,

    /// <summary>
    /// Cartridge Memory / Save File (AKA SRAM)
    /// </summary>
    CartRAM,

    /// <summary>
    /// Game data saved on cartridge
    /// </summary>
    CartROM
}
