using System.Linq;

namespace TrackerCouncil.Smz3.Data.Tracking;

/// <summary>
/// Class used to house byte data retrieved from the emulator at a given point in time
/// Used to retrieve data at locations in memory
/// </summary>
public class EmulatorMemoryData
{
    private byte[] _bytes;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bytes"></param>
    public EmulatorMemoryData(byte[] bytes)
    {
        _bytes = bytes;
    }

    /// <summary>
    /// The raw byte array of the data
    /// </summary>
    public byte[] Raw
    {
        get
        {
            return _bytes;
        }
    }

    /// <summary>
    /// Returns the memory value at a location
    /// </summary>
    /// <param name="location">The offset location to check</param>
    /// <returns>The value from the byte array at that location</returns>
    public byte ReadUInt8(int location)
    {
        return _bytes[location];
    }

    /// <summary>
    /// Gets the memory value for a location and returns if it matches a given flag
    /// </summary>
    /// <param name="location">The offset location to check</param>
    /// <param name="flag">The flag to check against</param>
    /// <returns>True if the flag is set for the memory location.</returns>
    public bool CheckBinary8Bit(int location, int flag)
    {
        return (ReadUInt8(location) & flag) == flag;
    }

    /// <summary>
    /// Checks if a value in memory matches a flag or has been increased to denote obtaining an item
    /// </summary>
    /// <param name="previousData">The previous data to compare to</param>
    /// <param name="location">The offset location to check</param>
    /// <param name="flag">The flag to check against</param>
    /// <returns>True if the value in memory was set or increased</returns>
    public bool CompareUInt8(EmulatorMemoryData previousData, int location, int? flag)
    {
        var prevValue = previousData != null && previousData._bytes.Length > location ? previousData.ReadUInt8(location) : -1;
        var newValue = ReadUInt8(location);

        if (newValue > prevValue)
        {
            if (flag != null)
            {
                if ((newValue & flag) == flag)
                {
                    return true;
                }
            }
            else if (newValue > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the memory value of two bytes / sixteen bits. Note that these are flipped
    /// from what you may expect if you're thinking of it in binary terms. The second byte
    /// is actually multiplied by 0xFF / 256 and added to the first
    /// </summary>
    /// <param name="location">The offset location to check</param>
    /// <returns>The value from the byte array at that location</returns>
    public int ReadUInt16(int location)
    {
        return _bytes[location + 1] * 256 + _bytes[location];
    }

    /// <summary>
    /// Checks if a binary flag is set for a given set of two bytes / sixteen bits
    /// </summary>
    /// <param name="location">The offset location to check</param>
    /// <param name="flag">The flag to check against</param>
    /// <returns>True if the flag is set for the memory location.</returns>
    public bool CheckUInt16(int location, int flag)
    {
        var data = ReadUInt16(location);
        var adjustedFlag = 1 << flag;
        var temp = data & adjustedFlag;
        return temp == adjustedFlag;
    }

    /// <summary>
    /// Returns if this EmulatorMemoryData equals another
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object? other)
    {
        if (other is not EmulatorMemoryData otherData) return false;
        return Enumerable.SequenceEqual(otherData._bytes, _bytes);
    }

    /// <summary>
    /// Returns the hash code of the bytes array
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return _bytes.GetHashCode();
    }
}
