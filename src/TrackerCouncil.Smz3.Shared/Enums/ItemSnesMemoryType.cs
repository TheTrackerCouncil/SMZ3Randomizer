namespace TrackerCouncil.Smz3.Shared.Enums;

public enum ItemSnesMemoryType
{
    // For items where the memory is the number of that held item (swords, keys, etc.)
    Byte,
    // For items where only having a value of 1 means you have the item (hookshot, cape, etc.)
    ByteSingleItem1,
    // For items where either the value 1 or 2 means you have the item (mirror, boots, etc.)
    ByteSingleItem12,
    // For items where any positive value means you have the item (bow)
    ByteSingleItemPositive,
    // For energy tanks (99 => 0, 199 => 1, etc.)
    WordEnergy,
    // For reserve tanks (100 => 1, 200 => 2, etc.)
    WordReserves,
    // For missiles, supers, and power bombs (5 => 1, 10 => 2, etc.)
    WordMetroidAmmo,
    // For items where a single bit in a byte reflects if it's grabbed (boomerangs, varia, etc.)
    ByteFlag,
    // For bottles where based 4 different bytes having a value > 0 sum up to the number of bottles
    Bottle,
    // For the flute where different bits in a single byte reflect having the flute
    Flute,
    // For items where the total is a sum of 2 byte values (Hyrule Castle key)
    Sum2Bytes,
    // For the HC map where different bits in a single byte reflect having the map
    HyruleCastleMap,
    // For heart contains (24 => 0, 32 => 1, 40 => 2, etc.)
    HeartContainers,
    // For rupees for Zora logic (250 => 1 three hundred rupee item, 500 => 2 three hundred rupee items)
    Rupees,
}
