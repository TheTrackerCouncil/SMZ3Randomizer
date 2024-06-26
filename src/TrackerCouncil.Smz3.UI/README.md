# SMZ3 Cas' Randomizer Cross-Platform

This project is the cross-platform version of the SMZ3 Cas' Randomizer. Currently this project is very much a work in progress, so the features and functionality are very limited in comparison to the Windows-specific version. This version is intended to be used for users on Linux (and potentially Mac).

## Current Functionality

- Generating roms utilizing settings configured in a YAML file, including sprites and selecting an MSU
- Launching the rom in a desired application
- Very basic commandline-based auto-tracking

## Desired Functionality

- A UI for generating new seeds and viewing the generated seed list
- Full-fledged item and map tracker windows
- Link, Samus, and ship sprite selector windows
- Full MSU support, including randomly picking tracks and shuffling tracks
- Multiworld support (may not support Windows + Linux in the same multiworld)

## Out-of-Scope Functionality (for now)

- Voice tracking
- Text-to-speech

## Setup

1. Install [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
2. Download the latest Linux version from the releases page
3. Make the SMZ3CasRandomizer file executable
4. Open a terminal and run SMZ3CasRandomizer
5. A randomizer-options.yml file will be generated in the working directory. Open that file in a text editor, then edit it.
    - **Z3RomPath**: Set to the path to your A Link to the PAst JP v1.0 ROM.
    - **SMRomPath**: Set to the path of your Super Metroid USA/JP v1.0 ROM.
    - **MSUPath**: Set to the path of the parent folder of all of your MSUs.
    - **RomOutputPath**: Base folder for where all generated seeds will go
    - **LaunchApplication**: Path to any script or application to use for launching a rom.
    - **LaunchArguments**: Any additional arguments that need to be passed to the LaunchApplication.
    - **SeedOptions**: This has various basic settings in regards to the seed, such as the requirements for completing the game, the keysanity settings, etc.
    - **AutoTrackerDefaultConnectionType**: Set to either Lua or USB2SNES to enable the console autotracking.
    - **PatchOptions**: Various options for patching the game, such as sprites and various Cas' patches.
      - **Sprites**: To select sprites, open the Sprites folder. Find the relevant rdc or ips file for the Link, Samus, ship sprite you want to use and put the path to it in the FilePath of the sprite category you want to update. You'll need to set "IsDefault" to false as well.
    - **LogicConfig**: Set various logic options to make the game easier or more difficult.

## Usage

1. Open a terminal and run SMZ3CasRandomizer. If the required settings are set, then it should walk you through the process of creating, playing, and deleting seeds
