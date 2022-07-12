# Memory Documentation

The distinctions between the different types of memory is very confusing. Different places refer to the memory in different ways, and the address locations vary per emulator and connection type. This documentation is meant to explain things to help remembering for the future.

Note that the memory in code references the snes9x memory addresses and will use offsets and other calculations to determine the appropriate memory addresses for BizHawk and USB2SNES.

## WRAM - SNES Console RAM
This is the basic RAM memory of the SNES itself, housing the current state of the game the player is currently in. It'll have information like their current location, current state, enemy data for the current location, etc.

### Accessing

snes9x - Starts at memory location 0x7E0000

BizHawk - Starts at memory location 0x000000 (Domain of "WRAM" has to be specified)

USB2SNES - Starts at memory location 0xF50000

## CartRAM (SRAM) - Cartridge Saved Data
This is data stored on the cartridge, which obviously includes save data, but it also has some other various data in it as well. One basic usage of this data is to determine the current game that the player is in. It can also be used to access the state data such as inventory and cleared locations of the game that the player is not currently in.

### Accessing
CartRAM can be accessed in snes9x directly via the memory address, but BizHawk and USB2SNES have the data compressed in comparison. snes9x has its memory stored in 0xA06###, 0xA07###, 0xA16###, etc. up to 0xBF7### whereas BizHawk and USB2SNES instead start at a memory address and go from 0x001###, 0x002###, 0x003###, etc. up to 0x03E###

snes9x - Starts at memory location 0xA06000

BizHawk - Starts at memory location 0x000000 (Domain of "CARTRAM" has to be specified)

USB2SNES - Starts at memory location 0xE00000

| snes9x    | bizhawk    | usb2snes   | 
|-----------|------------|------------|
| A06000    | 700000     | E00000     |
| A07000    | 701000     | E01000     |
| A16000    | 702000     | E02000     |
| A17000    | 703000     | E03000     |
| A17000    | 703000     | E03000     |
| BF7000    | 73E000     | E3E000     |

## CartROM - Cartridge Game Data
This is the data of the game itself, including the randomized data. So far this isn't used at all, but could theoretically be used for things like detecting pendant and item locations if we didn't have it from the generation.