# SMZ3 Cas’ Randomizer

> “This is the cas'est version of the rando you’ll ever see.” \
>— Diabetus, 2021

The **SMZ3 Cas’ Randomizer** is a fork of the original [Super Metroid & A 
Link to the Past Crossover Randomizer](https://samus.link/). Originally created to remove the decidedly _uncas’_ IBJ (Infinite Bomb Jump) technique, it's since grown into a project with a built-in tracker, accessibility patches, and other general quality of life changes added to make SMZ3 more casual and approachable..

A Windows installer for latest version of the **SMZ3 Cas’ Randomizer** can be 
found on the [GitHub releases] page. Linux and Mac versions are also available
with slightly limited functionality.

## Features
In addition to making IBJ completely optional, there is also:

 - Integrated voice-enabled/automated item & location tracker;
 - Built-in MSU-1 support for custom music packs;
 - Customizable logic for either casual or advanced play;
 - Various accessibility and quality of life patches;
 - Customizable ship sprites;
 - Patches for modern Super Metroid controls;
 - In game hint tiles and voice-enabled hints via tracker;
 - Basic Twitch integration for tracker responding to chat;
 - Multiworld support with individual player logic and game settings;
 - Sprites made by members of [Diabetus’](https://twitch.tv/the_betus) community and others;

 **Note**: Voice recognition and text-to-speech functionality is currently only available on Windows.

## Installation

### Windows
 - Download the latest version from the [GitHub releases] and run the installer
### Linux
 - Install [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
 - Download the latest version from the [GitHub releases] and extract into the desired folder
 - Make the SMZ3CasRandomizer file executable and run it
### Mac
 - Install [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
 - Download the latest version from the [GitHub releases] and move to the desired folder
 - In the terminal, go to the folder and execute the command `xattr -dr com.apple.quarantine SMZ3CasRandomizer.app`

## Credits
- Below you can find a list of all of the sprites included and their creators
    - [Link Sprites](https://github.com/TheTrackerCouncil/SMZ3CasSprites/blob/main/Sprites/Link/README.md)
    - [Samus Sprites](https://github.com/TheTrackerCouncil/SMZ3CasSprites/blob/main/Sprites/Samus/README.md)
    - [Ship Sprites](https://github.com/TheTrackerCouncil/SMZ3CasSprites/blob/main/Sprites/Ships/README.md)
- Various Super Metroid patches were pulled from the VARIA Randomizer by theonlydude
- [Diabetus](https://twitch.tv/the_betus) and [PinkKittyRose](https://www.twitch.tv/pinkkittyrose) and the members of their communities have helped test and stream with this fork

The original repository can be found at <https://github.com/tewtal/SMZ3Randomizer>.

[GitHub releases]: https://github.com/TheTrackerCouncil/SMZ3Randomizer/releases

## Hosting a SMZ3 Cas' Multiplayer Server
Interested in hosting your own server for multiplayer games? Take a look at the [Server Setup Documentation](docs/ServerSetup.md).