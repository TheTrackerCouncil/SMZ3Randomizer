# Auto Tracking Documentation

Auto tracking got to be pretty complex with multiple different forms, so this documentation is to try to keep it all straight for the future.

## Auto Tracking Connection Types

There are two forms of auto tracking connections: Lua and USB2SNES, each with their own distinct ways of connecting.

### **Lua Connector**

snes9x-rr (not the original version) and BizHawk both support Lua scripts, and using those we can create a connection between Tracker and the emulator using a TCP socket that Tracker opens up and listens for connections over. Once a connection is established, the Lua script and Tracker send JSON messages back and forth with read and write requests. The structure of these messages can be found in the LuaMessage class which is serialized/deserialized in the C# code.

#### **Available Actions**

- **read_block**: Reads the numbers of bytes specified in the Length starting at the memory location specified at Address in the specified domain (read below for more information about memory types/domains). Returns the bytes in the form of a base 64 string
- **write_bytes**: Writes an array of bytes in the WriteValues collection starting at the specified Address in the specified Domain.

#### **Lua Scripts**

The Lua scripts can be found under the AutoTracking folder. There are multiple versions because BizHawk and snes9x-rr both have different functions for executing different commands, and there are two different socket.dll files for 32bit vs 64bit. After recently looking at the crowd control scripts, I think these can be cleaned up a bit in the future. There are two main files that we need to worry about, though:

- **emulator.lua** - This houses the interactions with the emulator and has two unique versions: one for BizHawk, one for snes9x-rr. The file for the two snes9x-rr folders are the same. This is the main file I think can be cleaned up to have a single version by detecting whether BizHawk or snes9x-rr methods are available.
- **autotracker.lua** - This is a universal file that houses the interaction with Tracker via the socket connection. It reads messages from Tracker and parses them, then calls the methods in emulator.lua to interact with the emulator.

### **USB2SNES Connector**

This one works sort of opposite to the Lua Connector. For USB2SNES, the user will have the USB2SNES or QUSB2SNES application running on their computer, which runs a web socket server that Tracker connects to. There is documentation on the protocol for sending and receiving messages here: https://github.com/Skarsnik/QUsb2snes/blob/master/docs/Procotol.md

However, there are a couple things to note.

- Upon first connecting, USB2SNES will send a list of available devices that you can connect to. We could potentially give the user the option of which to use, but currently it simply connects to the first one using the "Attach" op code. Following that, we have to register ourselves with USB2SNES. There are also issues with USB2SNES returning bad data on first starting a game, so I implemented a 1 second delay before actually starting to read data.
- USB2SNES returns requested memory data via binary web sockets, which means that it's just an array of binary data with no context. Because of this, a response needs to be received for a memory request before making the next request.
- Conversely, when writing data you need to send two requests to USB2SNES. The first is a JSON message that tells it where to write to and how much data is being written, and the second is just the binary data to write.

## Memory Types

The distinctions between the different types of memory is very confusing. Different places refer to the memory in different ways, and the address locations vary per emulator and connection type. This documentation is meant to explain things to help remembering for the future.

Note that the memory in code references the snes9x memory addresses and will use offsets and other calculations to determine the appropriate memory addresses for BizHawk and USB2SNES.

### **WRAM - SNES Console RAM**
This is the basic RAM memory of the SNES itself, housing the current state of the game the player is currently in. It'll have information like their current location, current state, enemy data for the current location, etc.

#### **Accessing**

snes9x - Starts at memory location 0x7E0000

BizHawk - Starts at memory location 0x000000 (Domain of "WRAM" has to be specified)

USB2SNES - Starts at memory location 0xF50000

### **CartRAM (SRAM) - Cartridge Saved Data**
This is data stored on the cartridge, which obviously includes save data, but it also has some other various data in it as well. One basic usage of this data is to determine the current game that the player is in. It can also be used to access the state data such as inventory and cleared locations of the game that the player is not currently in.

#### **Accessing**
CartRAM can be accessed in snes9x directly via the memory address, but BizHawk and USB2SNES have the data compressed in comparison. snes9x has its memory stored in 0xA06###, 0xA07###, 0xA16###, etc. up to 0xBF7### whereas BizHawk and USB2SNES instead start at a memory address and go from 0x001###, 0x002###, 0x003###, etc. up to 0x03E###

snes9x - Starts at memory location 0xA06000

BizHawk - Starts at memory location 0x000000 (Domain of "CARTRAM" has to be specified)

USB2SNES - Starts at memory location 0xE00000

| snes9x    | bizhawk    | usb2snes   | 
|-----------|------------|------------|
| A06000    | 000000     | E00000     |
| A07000    | 001000     | E01000     |
| A16000    | 002000     | E02000     |
| A17000    | 003000     | E03000     |
| A17000    | 003000     | E03000     |
| BF7000    | 03E000     | E3E000     |

### **CartROM - Cartridge Game Data**
This is the data of the game itself, including the randomized data. So far this isn't used at all, but could theoretically be used for things like detecting pendant and item locations if we didn't have it from the generation.

#### **Accessing**
These all start at 0 from my understanding with the domain of "CARTROM" being required for BizHawk.

## Memory Ranges

This section is to document different memory ranges that are currently being used for and where to get additional info on what can be found.

### **Shared**

- 7E0020 (WRAM) - There's 1 byte here that can be used to determine if the game has been started or not.
- A173FE-A173FF (CartRAM/SRAM) - 2 bytes here can be used to determine whether the player is in Zelda or Super Metroid
- A26602-A26603 (CartRAM/SRAM) - 2 bytes here are used to figure out how many items have been gifted to the player by Tracker and other players and it has to be incremented with each new item given to the player.

### **Zelda**

The memory documentation I've found for Zelda is pretty extensive in comparison to SM. From my experience, there are two main pages to look at:

http://alttp.run/hacking/index.php?title=SRAM_Map - While this says it's for the SRAM for saves, the memory locations also correspond to the WRAM 7EF### range

http://alttp.run/hacking/index.php?title=RAM:_Bank_0x7E:_Page_0x00 - This page covers various state information of the player

- 7EF000-7EF250 (WRAM) - This section contains data on chests inside of caves, houses, and dungeon have been opened or not as well as data on if dungeons have been cleared.
- 7EF280-7EF480 (WRAM) - This section contains whether items on the overworld or from NPCs have been collected. This area also contains a lot of information about the player's inventory.
- 7E0000-7E0250 (WRAM) - This contains various state information about the player in the game, such as their location, what Link is currently doing, etc.

### **Super Metroid**

Metroid memory location can be found here: https://jathys.zophar.net/supermetroid/kejardon/RAMMap.txt 

- 7ED870-7ED890 (WRAM) - These are all of the metroid locations and if their items have been picked up or not.
- 7ED828-7ED830 (WRAM) - These are for if the bosses have been defeated. Interestingly, the previous Auto Tracker grabbed 8 bytes, but it only checks the first 4. The other 4 may be the other bosses.
- 7E0750-7E0B50 (WRAM) - This contains various state information about the player in the game, such as their location, inventory, etc.
