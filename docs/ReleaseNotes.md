## Changes in 9.8.0

- **Recreated UI**

  The UI has been completely rewritten from the ground up using a new framework. This new UI framework now sports a dark skin, some efficiency updates which prevents slowdown when resizing the map window, and the ability to have a unified cross platform UI. To work with this new framework, there are some slight changes to behavior as well. For example, when you open tracker, the rom list window will now be hidden until tracker is closed. The Shaktool mode has also now been changed to sport a larger single gif instead of a series of them.

  ![image](https://github.com/TheTrackerCouncil/SMZ3Randomizer/assets/63823784/59f52f2d-4a02-469d-bdcc-c4bdb9ac53fe)

- **Generation Window Updates**

  The generation window has been redesigned into a multi-tab layout. The goal with this change is to have the UI be a little less daunting for newcomers as well as have the primary settings you change each run such as the sprite and MSU to be more easily accessible. Copying config strings from other people has been updated to alter the settings in the window, and you can now see a list of all of the settings at a glance on the main window.

  To go along with this, a new feature has been added: presets for quickly applying logic and patch settings. There are multiple built-in presets which you can select from, and you can even create your own preset by clicking on the down triangle menu button next to the Generate button.

  ![image](https://github.com/TheTrackerCouncil/SMZ3Randomizer/assets/63823784/fe1f00d9-cc05-42bc-9fea-c34a68152b76)

- **MSU Randomizer Updates**

  SMZ3 has been updated to utilize the latest functionality of the MSU Randomizer. You can now adjust the weighting of particular MSUs, and there are different shuffle styles, including options for shuffling jingle tracks or all tracks together.

- **Expanded Linux Version and Mac OS Port**

  As stated before, the new UI framework allows for the Linux version to have a fully functional tracker UI, including the item, location, and map windows. Additionally, for Mac OS X users with the M1 or newer arm processors, there is now a Mac port you can use. However, there is no voice tracking or text to speech for the Linux and Mac ports, so keep that in mind. For instructions on how to setup the Linux and Mac versions, view the [installation section of the main GitHub page](https://github.com/TheTrackerCouncil/SMZ3Randomizer).

- **Miscellaneous Updates and Fixes**

  - The application has been updated to .net 8. For Windows users, it should be automatically installed when updating.
  - The Specky clip comment will now only be said once per run.
  - The options window has been slightly re-organized so that the settings are in more appropriate sections.
  - Fixed an issue where locations being set to "progression" weren't having random items associated with them.
  - There are now additional Lua connectors which should be compatible with the EmoTracker and Crowd Control. On top that, the Lua scripts have been updated to be more robust. SNI has also been added as a supported connector.
  - Fixed an issue where the disabled voice detection was broken on Windows.
  - Fixed an issue where default sprites weren't being announced correctly by tracker.
  - Fixed an issue where keycard types would be spoiled when viewing them in world.
  - Fixed an issue where the pre-Crocomire line would be stated in keysanity without the boss keycard.
  - Bumper cave should now auto track.
  - The hi jump lobby missile location now requires bombs to access to avoid people accidentally soft locking.
  - Added a warning when trying to copy a seed number from a randomizer settings string from an old randomizer version that may produce different results.
  - Fixed an issue where tracker failed to reconnect to chat when she lost the connection to Twitch.
  - Tracker will now automatically track pegs you hammer on the way to Peg World.
  - Tracker will also count how many Hyper Beam shots it takes to bring down Mother Brain.

### Changes in 9.8.1

- The previous MSU track should no longer resume after "Samus Fanfare" plays (e.g., after resetting out of Wrecked Pool).
- MSU tracks should more reliably start over, without fading in, after the packs have been shuffled.
- The timer in the Tracker window can now be disabled, for when you're using your own timer.
- Tracker will now combine "Pop"s more often when auto-tracking Peg World.
- Tracker will now try to combine messages when a lot of items are picked up at once, though she might not be thrilled about it.

### Changes in 9.8.2

- Fixed an issue that caused a crash when opening the MSU Randomizer window.

### Changes in 9.8.3

- Added a button to the top right of the tracker window to resize the window to best fit the current selected layout.
- Added a progress bar to the sprite download window and made it so the sprite download could be cancelled.
- Fixed an issue that would cause songs with the name "null" to not work properly with the MSU randomization.
- Fixed an issue where new sprites wouldn't be downloaded upon first setup of SMZ3.
- Added a small surprise feature that I'm not sure everyone will be *content* with.

### Changes in 9.8.4

- **Tracker Speech Window**

  Ever want tracker to stare into your soul as she mocks you while playing SMZ3? Well wait no longer! With the new Tracker Speech Window, a tracker image drawn by the talented [DrDubz](https://linktr.ee/mightydubz) will be displayed and be slightly animated when she starts talking. On the tracker window, you can go to View -> Tracker Speech Window to open a window that looks like this:

  ![image](https://github.com/user-attachments/assets/f9d916db-71cf-4f57-afa6-3316bd7189f6)

  For streamers, you should be able capture this window and chroma key it to remove the background and add to your scene:

  https://github.com/user-attachments/assets/f16dbf17-12cd-4720-9652-7d6c790ccf63

  By default, the color you'll need to chroma key is #483D8B, but you can change the color and disable tracker's bounce animation in the settings. You can also right click on the tracker speech window to copy the chroma key hex for using in OBS.

  This feature is pretty new, and will probably be added to in the future, so be on the look out for future updates!

- **Miscellaneous Updates and Fixes**

  - Fixed an issue where new sprites wouldn't download properly and where sometimes all sprites might be redownloaded from GitHub.

### Changes in 9.8.5

- **New Shinespark Cheat**

  Annoyed at missing when getting the Maridia entrance shine spark item? There is now a cheat to enable a shine spark at any time. Just say "Hey tracker, charge a shine spark!" while cheats are enabled. 

- **Tracker Reaction Types**

  Currently no voice lines utilize this, but there is now a functionality to specify the reaction image that tracker will use for voice lines. If anyone wants to add their own voice lines that use this, simply update your voice lines in the config files like this:

  ```
  - Text: <break time='3s'/> Oh I'm sorry. Was I supposed to care about Reserve Tanks?
    Weight: 0.5
    TrackerImage: Bored
  ```  

  At the moment, the only tracker image types are default, bored, and wtf. Once a voice line is finished, she will automatically switch back to the default images.

- **Miscellaneous Updates and Fixes**

  - When requesting a hint for a single specific location, tracker will now distinguish between mandatory, nice to have, and junk items.
  - Hint tiles and viewed items now share the same command for triggering them to avoid mix-ups. Basically, saying "Hey tracker, clear that hint tile" or "Hey tracker, clear that marked location" will both work for both hint tiles and marked locations.
  - Fixed an issue with the tracker responses to specific MSUs and tracks not always working.
  - Updated the tracker location list to properly scroll again.

### Changes in 9.8.6

- **Keysanity Minimal UI**

  A new UI has been created for the keysanity that is meant to be the same width as the advanced layout so that streamers don't need to change their stream layouts for both.
   
  ![image](https://github.com/user-attachments/assets/fa4ee127-883f-4624-aefa-3d53f8e3b7e1)

- **Miscellaneous Updates and Fixes**

  - Fixed an issue where tracker stopped talking after peg world.
  - Updated hint tiles for keysanity to no longer give hints for crystal dungeons being mandatory to be more in line with regular mode.

### Changes in 9.8.7

- **Hey Tracker, track Halloween**

  There's a bit of a spooky surprise for folks who have tracker's sprite displayed! If anyone doesn't want to use it though, there's an option in the tracker profiles where you can change back.

- **Aga Ledge/Lumberjack Tree is Actually in Logic Now**

  Due to an issue going back _years_ now at this point, Aga ledge was never actually possible to have progression generated on them. This has now been resolved. Note that with this change, there's a possibility that using seeds from other people from before this version may not generate the same.

- **Miscellaneous Updates and Fixes**

  - Fixed an issue where push-to-talk mode wasn't working.
  - Fixed an issue with plando files not loading properly.
  - Fixed an issue where the banner to disable update notifications wasn't taking while the settings menu option was.
  - Added additional commands for the shine spark cheat.

### Changes in 9.8.8

- **Miscellaneous Updates and Fixes**

  - Removed weighting for Aga ledge, making it very slightly less likely to have progression (around 7% instead of 7.5%)
  - Changed the default tracker back to the original one. For anyone who would like to continue using the Halloween tracker sprites, you can select them in your tracker profiles in the settings.

## Changes in 9.7.0

- **Push-to-talk**

  Unfortunately due to technical limitations, the original voice recognition method used by tracker doesn't allow changing the microphone used. In order to allow people to change the microphone used by tracker, we have implemented an option to enable a push-to-talk mode. With this mode, you can either use the default windows microphone or specify a specific microphone, then set a button to use to initiate push-to-talk.

  This only works with keyboard keys, so if you want to use a controller button, you would need to use an external program to bind your controller to the keyboard key used.

- **Auto Updating Tracker Configs & Sprites**

  The tracker YAML configs and all of the sprites used by the SMZ3 Cas' Randomizer have been moved to separate repositories to make them easier to manage as well as allow the SMZ3 Cas' Randomizer to automatically update the configs and sprites on launch. This means that basic tracker line changes and new sprites won't require new builds of the randomizer. Furthermore, the config files now utilize schemas so that if using Visual Studio Code and the YAML plugin, it'll highlight errors automatically.

  You can find the repositories for the configs and sprites below:

  - https://github.com/TheTrackerCouncil/SMZ3CasConfigs
  - https://github.com/TheTrackerCouncil/SMZ3CasSprites

  With these changes, we hope it'll be easier for people to edit and even contribute tracker lines and sprites. If anyone doesn't want the configs and sprites to be automatically updated, this can be disabled in the settings.

  For anyone who previously created new tracker profiles, then you can read [this document](https://docs.google.com/document/d/1lwKHfxYujvY--pmsWLn5dIBxqDqTvM1znA_xwPTnRac/edit?usp=sharing) for information on how to update your profile configs to make use of the schemas as well as edit them for the new tracker mood setup from version 9.6.1.

- **Two New MSU Display Styles**

  For the MSU information window and output text file, two new styles were added to present more information. There is a new single line style that includes the MSU name and creator. Another style displays the all of the MSU information in sentence form.

- **Miscellaneous Updates and Fixes**

  - Config files that have extra fields in them will no longer error out when loaded.
  - Various tracker line have been added and updated.

### Changes in 9.7.1

- Fixed an issue when saving tracker and reloading after viewing a hint tile about the medallion requirement for Misery Mire or Turtle Rock

### Changes in 9.7.2

- Fixed an issue where Tracker could give misleading hints when asking about cleared crystal dungeons.

### Changes in 9.7.3

- **Sandpit Platforms**
  - Added a new Cas' patch for sandpit platforms to make it easier to traverse Maridia without the space jump. This setting doesn't modify any of the logic.
  ![image](https://github.com/TheTrackerCouncil/SMZ3Randomizer/assets/63823784/810b133d-fa4a-4944-a03d-1a53dc4fb097)

- **Miscellaneous Updates and Fixes**

  - Fixed an issue that could cause UI layouts to be duplicated in the dropdown
  - Fixed an issue where previous settings strings could apply to multiworlds and plandos
  - Fixed the credits music not playing on bsnes and on certain hardware
  - Fixed stutter sounds that could happen when changing tracks in SM on the FxPakPro
  - Fixed an issue with using QUSB2SNES and FxPakPro on certain systems
  - Fixed an issue that could prevent certain item location settings not generating successfully
  
### Changes in 9.7.4

- Fixed an issue that could cause tracker to conflict with the MSU Randomizer
- Fixed detection of Metroid deaths to work when manual reserve tanks are enabled

## Changes in 9.6.1

- **Advanced Tracker Mood Functionality**

  Previously tracker moods were simply dictated how tracker would respond to "Hey tracker, how are you?" However, this functionality has been expanded so that all responses can be changed based on the mood. For each YAML file, such as responses.yml, you can now create responses.Sassy.yml, responses.Rude.yml, or any mood you want, and tracker will pick from all of the moods available.

  Currently this is mostly being used to manage lines that were already there, but this will be expanded in the future.

- **Miscellaneous Updates and Fixes**

  - Tracker will now respond when you pick up items that were set to be at a particular location.
  - Tracker can now have special responses to specific track numbers, MSU names, and MSU song names.
  - The final batch of Link sprites have been added.
  
## Changes in 9.6.0

- **Automatic Visible Items and Hint Tile Tracking**

  When you read a hint tile or view an item, that will now be automatically marked if the option to auto track viewed events is enabled. If the auto track viewed events option is disabled, you should be able to say "Hey tracker, take a look at this" similar to viewing the map and marking the dungeon rewards. (**Note:** It just occurred to me that the verbal command for this has not been thoroughly tested. I'll do so before the full release.)

  For visible locations and hint tiles for a single location, the locations will show up in the marked items location list similar to you marking an item at a location. For hint tiles that are about a dungeon or a group of locations, there is a new list that will appear below the marked items. On the map, locations that are known to have something mandatory or nice to have will be marked with a gold diamond in the corner. Locations that are known to have only junk will have a red x.

  ![image](https://github.com/Vivelin/SMZ3Randomizer/assets/63823784/f6b1784e-9abc-49b9-88ea-d62cb0d07413) ![image](https://github.com/Vivelin/SMZ3Randomizer/assets/63823784/38b242a5-500a-4e5a-96a6-e5db3bf528c8)


  Note that **all** things will show up automatically when viewed, even 20 rupee items or dungeons that are not required. This is so that the player can choose to leave it as a reminder or clear it themselves. However, there are new commands to make this easier. If you don't care about a marked location, you can say "Hey tracker, clear that marked location" and if you don't care about a hint tile, you can say "Hey tracker, clear that hint tile" to clear the relevant location(s) for you.

  Note that some visible locations may have been missed and will need to be added later.

- **Hint Voice Command Updates**

  You can now ask tracker about what a hint tile says in case you don't want to go there. You can ask tracker things like "Hey tracker, what does the eastern palace hint tile say?" for that, and she'll automatically mark it for you as if you read the hint tile.

  The "Hey tracker, where should I go?" and "Hey tracker, is there anything in X?" hints have been updated to use the same logic as the hint tiles, so they should be more accurate as to what's actually required or not. You'll now need to actually pay attention to what tracker says, though! Now there are different responses for "mandatory" and "nice to have" in a dungeon whereas the text was the same before.

- **Miscellaneous Multiworld Updates and Fixes**

   - Due to a bug when the option to send out items when players complete the game was disabled as well as because players can get stuck if completing Metroid second and save before fighting Mother brain, multiworld games now are forced to have items sent out when players beat the game.
   - When a player beats the game, some items will no longer be sent out to avoid the flood of new items. _Extremely_ useless items such as Zelda bomb/arrow drops and lesser rupee items will be skipped.
   - Fixed a bug where players would receive their own items when completing the game.
   - Fixed an issue where the randomizer would sometimes crash when the connection is lost to the server.
   - Prevented repeated error spam when receiving errors connecting to the server.

- **Miscellaneous Updates and Fixes**

  - The weights of the pedestal and lumberjack ledge locations have been tweaked to make them a little more likely to have progression.
  - You can now mark generic small keys and maps at locations in the case of playing a keysanity game.
  - Fixed an issue with fighting Aga 2 when open pyramid is enabled.
  - Fixed an issue with the timer not resetting when first starting the game if items were tracked and undone.
  - Fixed an issue where the MSU would not reset when hitting Start+Select+L+R at times
  - Fixed an issue that's caused the tracker lines when entering pendant dungeons and castle tower lines from ever showing up, though they won't come up 100% of the time like before.
  - Various tracker line have been added and updated.
  - Added 76 new Link sprites, including a ConstantineDTW sprite by Rejax.
  - Added 2 new ship sprites by Phiggle.

## Changes in 9.5.0

- **Sprite Selection Window**

  For selecting Samus, Link, and Ship sprites, there is now a window that will pop up to display the sprites to make selecting sprites easier. You can filter, favorite, and hide sprites when selecting, and you can even select the Random button to have it pick a sprite from the current sprites being displayed in the list. Currently it won't remember your filter options and if you want to select a Random sprite you'll need to open the sprite window each time, but stay tuned for more updates to this feature in the future to help make that process a bit smoother!

- **MSU Selecting Updates**

  You can now favorite MSUs as well in the MSU list! Furthermore, there has been a change in a bit of the functionality. Selecting an MSU will no longer randomize the alt tracks in case you want to do some manual tweaks on what alt tracks you're using. A common use case of this is for the Super Metroid escape sequence where some MSU creators have versions with and without the alarm sounds. If you want to select a single MSU and _do_ want the alt tracks shuffled in, then simply use the "Create Shuffled MSU" option and only select the one MSU.

  Furthermore, if there's an MSU you don't want the alt tracks shuffled in, even when using the MSU shuffling, you can now right click on the MSU in the MSU list to open the MSU details and select the new "Leave Tracks Alone" option.

- **Current Song Information**

  If you're using an MSU and have auto tracking enabled, tracker can now know the current song being played. You can ask tracker "Hey tracker, what's the current song?" or "Hey tracker, what MSU pack is the current song from?" When tracker is open, you can click on "View -> Current Song" to open a window that will display all of the information as you go around. Or, if you'd prefer, if you go to the "Tools -> Options" and set the "Current song output path" to have the text information be written to a file which can be read by OBS.

  The information and format can be customized by changing the "Current song display style" in the Options. The vertical format will display all of the information, including the MSU pack name and creator whereas the horizontal format is more abbreviated. Note that this change will require restarting tracker.

  Note that the song information is only updated around once every 2 seconds due to technical limitations.

- **Miscellaneous Updates and Fixes**

  - Hint tiles have been removed from plandos for now to avoid issues with generating plandos.
  - If you give yourself a starting flute, you will now have to manually activate it. This is because of an issue where before if you got the shovel you would not be able to use it.
  - The dungeon reward type is now automatically set when using auto tracking to assist with folks playing keysanity.
  - The latest VARIA Samus and Ship sprites have been pulled in and around 50 random Link sprites have been pulled in from ALttPR. 50 or so sprites will be added to the next releases until we're in sync with ALttPR.

### Changes in 9.5.1

- **MSU Resume Updates**

  Super Metroid now has MSU resume support! If you enter an item/elevator room and return to an area with the same music within 30 seconds, the MSU will continue where it left off. Furthermore, now for both Zelda and Super Metroid, when the MSU is reshuffled, then the MSU resume state will be cleared out so that the new song won't be started in the middle.

- **Zelda Pickup Options**

  There are now options to adjust the pickups for Zelda to change the enemy drops, tree pulls, and crab prizes. You can modify this to be randomized (default), vanilla, easy, or difficult.

- **Miscellaneous Updates and Fixes** 
  - A notification bar will now appear at the top of the rom list window when there is a new version of SMZ3 released. This can be dismissed for a single release and can also be disabled entiredly in the settings.
  - Filter options for sprites are now saved, and when a random sprite is selected it will no longer spoil what the sprite is before starting the game.
  - Pickups, starting inventory, hint tiles, and other in game text can now be modified in plandos. Special messages can also be added to specific locations in plandos.
  - Added Shak's Stash ship sprite by Phiggle, a couple of Link sprites submitted by [mcent1](https://github.com/mcen1), and 50 random Link sprites from ALttPR
  - You can now update the IP address used to connect to QUSB2SNES.
  - The uncle and the adjacent secret passage chest will no longer be treated as Hyrule Castle when it comes to hint tiles.
  - Triggering peg world and go mode will now cause tracker to stop talking and immediately announce peg world/go mode. This is to solve an issue that could happen when tracker has a lot of messages queued up to say when peg world or go mode are initiated.
  - Fixed some issues that could cause you to not be able to re-enable voice detection after disabling it.
  - Fixed an issue where favorited ship sprites would be forgotten.
  - Fixed the issue that would allow you to go to Tourian without defeating Draygon.

### Changes in 9.5.2

- **Auto Track Dungeon Rewards and Requirements when Viewed**

  By default going forward when auto tracking, dungeon rewards will be automatically tracked when looking at the map and dungeon requirements will be automatically tracked when near the dungeon entrances. This feature will be expanded to visible items and hint tiles in the future. While enabled by default, this feature can be turned off in the settings by unchecking the "Auto Track Viewed Events" option.

- **Customizable Launch Settings**

  Previously when launching a rom, SMZ3 would launch the rom using the default application specified in Windows for sfc files. Now in the options menu you can specify both a Launch Application and Launch Arguments. The Launch Application is the executable or script you want to execute, and the Launch Arguments are commandline arguments passed into the application. If the arguments field is empty, the path to the rom will be passed. In the arguments field, you can use %rom% for where the rom path will injected, but if not specified, then the rom path will be appended to the end of the arguments.

  Using this, you can have SMZ3 use a different version of snes9x-rr from Archipelago, launch the rom in RetroArch, or even create your own custom script which will make sure that QUSB2SNES is running before launching the emulator.

- **Updated Settings Format**

  This is just a notice that the format used to save the settings for SMZ3 has changed from JSON to YAML. This _should_ be seamless and preserve all settings, but I wanted to bring it up just in case.

- **Linux Version**

  As of this release, there is now a command line Linux version with support for rom generation and auto tracking. Manual tracking is not currently supported, and there is no text-to-speech or speech recognition. When launching the first time, it'll create a randomizer-options.yml file in the current working directory. You'll need to edit that file to specify the paths to the Metroid and Zelda roms and edit any settings you'd like.

  In order to use the Linux version, you'll need to make sure you have .net 7 installed on your computer. You can either run it by making the SMZ3CasRandomizer file executable and running it in the terminal of your choice or by running the command "dotnet SMZ3CasRandomizer.dll".

- **Miscellaneous Updates and Fixes** 
  - Fixed an issue with Super Metroid MSU resume when being in an item/elevator room for a long time. This mostly affected the Chozo bowling alley room.
  - Fixed an issue where Ganon wouldn't say the location of the silver arrows
  - Removed the text border around the Triforce screen text
  - Added 50 additional Link sprites

### Changes in 9.5.3

- Fixed an issue that could cause copying to clipboard to not work
- Fixed an issue where the selected launch option was being ignored
- Fixed an issue where viewing the map in key sanity would repeat "looked at nothing" messages
- Added some additional tracker responses
- Added 50 additional Link sprites
- Refactored some back end parts in preparation for future features

## Changes in 9.4.0

- **MSU Randomizer Integration**

  The MSU Randomizer code has been integrated into the Cas' Randomizer. You'll specify an MSU directory, then you can either select from MSUs in that directory, have it randomly pick an MSU for you, shuffle multiple MSUs together, or even continuously shuffle the MSUs together every 60 seconds! You can even use this to combine split SM and Z3 MSUs into a single SMZ3 MSU.

  Along with this functionality, you can ask Tracker what song is currently playing for an area. You can ask "Hey tracker, what is the current song for Hyrule Field?" for the full details of the song or "Hey tracker, what MSU pack is the current Hyrule Field theme from?" for just the MSU name and creator. For Tracker to be able to pull this information, MSU packs will need to have [MSU Randomizer YAML files](https://github.com/MattEqualsCoder/MSURandomizer/blob/main/Docs/yaml.md) provided with them. If you aren't sure about how to write the YAML files, you can try downloading the [MSU Scripter](https://github.com/MattEqualsCoder/MSUScripter).

- **Miscellaneous Updates and Fixes** 
  - Broadened the area for Specky Clip detection.
  - Updated the top left of Bubble Mountain to not be accessible with just high jump boots with easy wall jump logic.
  - The project has been updated to .net 7. It should be automatically installed when you install this update.
  - Fixed an issue with rejoining multiworld games before being in the game not working properly.

## Changes in 9.3.0

- **MSU Updates**
  
  The MSU code has been updated to match the implementation of the current beta version of mainline SMZ3. This adds MSU resume functionality and fixes a few corner cases that previously existed. It also matches the behavior of the ALttPR MSU functionality moreso than the previous MSU implementation. Due to this however, the native MSU track order has been reversed with the Zelda tracks now being from 1-63 and the SM tracks as 101-140. The Randomizer should take care of swapping the tracks for you, however.

- **Miscellaneous Updates and Fixes** 
  - A previous issue that prevented multiworld games from saving for async games has now been resolved.
  - Fixed an issue with Power Bomb item notifications being messed up in Super Metroid.
  - Removed the requirement for the 4 Super Metroid bosses to be able to be defeated before GT is in logic if the number of crystals to get into GT is fewer than the crystals needed to defeat Ganon.
  - Changed the quarter magic option from the Cas' settings to the Logic settings so that they will be preserved if you give someone a config string. Note that if you previously had quarter magic disabled, you will need to disable it again.
  - Added additional hint tile possibilities.
  - Changes have been made in the codebase to help make further changes easier.
  - Updated tracker responses.

### Changes in 9.3.1

- **MSU Hardware Fixes**

  A couple of updates were pulled in from the mainline SMZ3 to address issues found when playing on hardware.

  - The Super Metroid and SMZ3 Credits MSU tracks now properly play on SD2SNES/FXPak
  - The SMZ3 Credits now properly fall back to the PCM track if a song does not exist

- **Multiplayer fixes**
  - Updated tracker to no longer remark on deaths when killed by the game service
  - Prevented death linked players from tracking deaths to the server
  - Fixed broken server logging that would cause error popups on the client end

### Changes in 9.3.2

- Various tracker voice line updates.

### Changes in 9.3.3

- Fixed an issue where it would get stuck in a loop on first setup when the rom output path did not exist
- Fixed an issue on first setup where it would not display the rom creation options after inputting the correct settings
- Added tracker lines for the Specky Clip

## Changes in 9.2.0

- **Auto Save on Metroid Deaths**

  Have struggles with playing Metroid? No worries! You can now have an option enabled to save your progress when you die, similar to Zelda. No longer do you need to worry about losing those progression items because you died to Ridley! When you die, you'll go back to the last save station or back to your ship if you haven't hit up a save station since transitioning from Zelda. *This does not work with Start+Select+L+R.*

  I'm sure this won't open the door to some time saves via tactical resets...

- **Multiworld Death Link**

  Do you love causing your friends misery? Well good news for you, since now in multiworld games it can now be set to where if one player dies, all the other players die! With the auto save feature, this functionality isn't _as_ frustrating as it would have been had you lost all of your progress.

- **Backend Rework**

  In order to take baby steps to restructuring the way the game logic is setup to make logic changes easier, the Metroid locations have been reworked in the code. This _shouldn't_ cause any real changes in behavior, but be sure to let us know if you notice anything odd come up. This will cause it so that old seed numbers may act different if applied in this new version.

- **Miscellaneous Updates and Fixes** 
  - The "refill my rupees" cheat now sets your rupee count to 2000
  - You can now actually force clear rooms, dungeons, and areas that are out of logic
  - Retrieving half/quarter magic in Super Metroid should now refill your magic in Zelda
  - Updated the multiworld logic to attempt to resend items that may have been missed due to dropped messages to the emulator
  - Changed the auto tracker timing for locations and bosses to be on a bit of a delay to allow other, more important checks to happen more frequently (such as checking the current game or checking for tricks)
  - Hint tiles will now properly make sure you can retrieve all required crystals in the case of the GT requiring fewer crystals than defeating Ganon
  - Various updates to tracker responses

### Changes in 9.2.1

  - Fixed an issue with picking up half and quarter magic in Super Metroid

## Changes in 9.1.0

- **Moat Speed Booster Fly By Advanced Logic Option**

  Up until now, the default logic has been that one of the ways to get to the Wrecked Ship was by using the speed booster to shine spark over the moat. This can be a bit tricky similar to the Parlor Speed Booster Break In. Because of this, by default this will no longer be in logic, and there is now a checkbox in the advanced logic section to make it in logic again.

- **Hint Tile Updates**

  There have been a few corner cases which have made hint tiles incorrectly state that areas were mandatory when they were not.

  - Previously the first two intended power bombs and first super missile could be marked as mandatory even if there were others accessible. This has now been fixed.
  - Due to the complex nature of sword logic (the first two swords you find are progression, but the second two are not), a change has been made so that now if an area does not have any other mandatory items like the bow, and a sword is located there, the hint tile will say that the area has a sword so that you can discern if it is mandatory for the seed or not yourself.
  - Silver arrows are now deemed as mandatory when it comes to hint tiles.

- **Sprite Additions** 

  - Link
    - Dark Matter
    - Elfilin
    - Goemon
    - Metieon
    - Milly
    - Saria
    - Spamton G. Spamton
    - Susie
    - Tunic
  - Samus
    - KMaria Pollo
    - Mario (8-Bit version)
    - Mario (8-Bit version with modern colors)
    - Plissken
  - Ship
    - Dr. Wily's Ship

- **Multiworld Fixes** 
  - Fixed an issue where items would not show up in UI when there are no instances of it in the current player's world.
  - Fixed an issue where tracker would try to give the player items when they aren't fully booted into the game where it'll accept new items, causing you to lose out on those items.
  - Update to mark the player as beating both final bosses when one of the two was actually detected.

- **Miscellaneous Updates and Fixes** 
  - Chat usernames should use the proper casing now, which should hopefully fix Tracker's pronunciation for a lot of names.
  - Hyrule Castle should now be marked as completed and the Ball & Chain soldier as defeated when opening the chest in Zelda's cell.
  - Tweaked Tracker's wording with the "Way of the Hero" hint for an area to avoid confusion.
  - It is now possible to create custom commands to initiate go mode.
  - Removed the item hint that referred to gloves as magical and added some new ones to replace it.
  - Added a cheat to set up crystal flash testing. Just say "Hey tracker, setup crystal flash requirements" or "Hey tracker, ready a crystal flash" after enabling cheats.
  - Fixed a couple starting inventory setting options.

## Changes in 9.0.0

- **Metroid Control Updates**

  Various optional patches have been created to update Metroid's controls to work more in line with the Gameboy Advance Metroid games and Metroid Dread.

  - Unified Aim Button & Quick Morph - Instead of having separate aim up and aim down buttons, it will now be a single aim button. You will default to aiming up, and pressing up/down on the d-pad will then change the direction in which you're aiming. With the newly freed button, it will now be a quick morph button. Note that it's not instantaneous as you still have to go through the standing -> crouch -> morph animations and you need to hold the button for a few frames rather than a quick tap.
  - Various item cancel button behavior options
    - Hold to keep super missiles/power bombs selected - While you have the item cancel button held, you will always have super missiles (or regular missiles if you are out of supers) or power bombs selected. If you are holding the button while morphing or unmorphing you will switch to supers or power bombs automatically.
    - Hold to keep missiles/super missiles/power bombs selected - This is the same as above, except for when you are not morphed, if you hit the item select button you can switch between missiles and super missiles. It should remember your last selected option for the next time you hold down the button.
    - Press to toggle supers/power bombs - This works similar to the item switch button, but is limited to just supers and power bombs to make it easier to select what you need.
  - Auto Run - This will basically reverse the run behavior so that you run by default and you can hold the run button to prevent yourself from running.

- **Saved Metroid Controls**
  
  You can now enter your preferred Metroid button mappings when generating your seed rather than do it in game. These settings will be saved, meaning you can set them once and never have to set them again! The labels will also change based on your Metroid control settings.

- **Starting Inventory & Other Item Setting Tweaks** 
  
  - You can now specify what items you start with! You can give yourself progression items, swords, mail, beams, or even things like missiles, hearts, and bombs. Whatever items you select will be will not be spawned in the world.
  - Items that were previously missing from the early item options, such as silver arrows, can now be selected as early items or can be selected to be placed at specific locations.
  - You can now have quarter magic added to the item pool.
  - You can also have bottles be randomly filled when you pick them up as well as have the fairy bottle trades give you random filled bottles instead of just a green potion bottle.

- **Goal Settings** 

  To make seeds faster, you can now change the goals for completing either of the two games as well as have the pyramid be open by default. Note that this means that going forward by default Ganon will now require 7 crystals, but this can always be changed to match vanilla behavior.

- **Sprite Updates** 

  - Link
    - Randi (Secret of Mana)
    - Red (Pokemon)
    - Shak-O-Lantern
  - Samus
    - Kaizo (Kaizo Hack)
    - Kid Goku
    - Master Hand
    - Ronald McDonald
    - Tetromino
    - Samus (Wireframe)

- **Miscellaneous Updates and Fixes** 
  - The Crocomire Escape location logic was updated to require the jump difficulty config to be set to hard jumps for the location to be in logic if you don't have grapple or space jump.
  - Updated MSU support to be more consistent between BSNES and SNES9X. Previously there were issues with the credits music and when transitioning games. You may notice subtle changes with the sound cutting out when hitting Start+Select+L+R in Zelda or when transitioning between games.
  - Added a "Look at This" command for Misery Mire and Turtle Rock required medallions. Note that due to timing on how frequently tracker gets updates from the game, quickly peeking and immediately turning around may not be picked up.
  - Fixed an issue where looking at the map immediately after transitioning games and asking tracker to record the dungeon rewards wouldn't be picked up properly.
  - All other dungeon locations will now be cleared when all treasures have been looted and the boss has been defeated.
  - Fixed an issue where Tracker wasn't always checking if an item like the mushroom, magic powder or bottle could lead to another item when it came to asking for location hints.
  - Fixed an issue where using Start+Select+L+R in Zelda would cause you to lose your selected item when transitioning back to Zelda.
  - Updated the game to where you will automatically select the spazer or plasma when they are picked up in Zelda.
  - Fixed an issue where in some seeds hint tiles could refer to dungeons as required even if they weren't.
  - Updated randomization to minimize potential patterns with dungeon rewards and medallions.
  - Reordered the generate seed window.
  - Fixed an issue where the tracker window locations would be saved out of bounds.
  - Added rom validation to make sure you're using the correct Link to the Past and Super Metroid roms.
  - Fixed an issue where copying a setting string would have tracker say the incorrect sprite names.
  - Fixed an issue with marking remaining dungeons as crystal not working in keysanity mode.
  - Fixed an issue where asking where an item was would not give the correct hint if it's behind a boss that has not been defeated, like Kraid or Phantoon.
  - Implemented a workaround to make Twitch content polls more consistent by ignoring when Twitch says polls are "terminated."
  - Various tracker line updates by @TheRealFragger, @CPColin , and @MattEqualsCoder.

## Changes in 8.0.0

- **Multiworld**

  Starting in this version, you can now host and join multiworld games! Each player is able to have their own settings, meaning each player can pick their own logic settings, item preferences, sprites, MSU patches, and even if they are playing keysanity or not. This even supports both live multiworlds and async multiworlds. Note that in order to prevent issues with the server running out of memory, live multiworlds will be closed if no player receives an item within an hour and async multiworlds will be closed if no player receives an item within 30 days.

  **NOTE:** This requires auto tracker, so make sure auto tracking is working for you before trying out a multiworld game!

  You'll notice when you first start SMZ3 now, there will be two tabs: one for singleplayer, one for multiplayer. To host your own multiworld game, simply go to the Multiplayer tab and click on Create game to see the following window for setting up a new multiworld game:

  ![image](https://user-images.githubusercontent.com/63823784/209819290-f21c50d8-72e2-4422-9e4c-3a25dd0f59fd.png)

  If you don't see a url populated, click on the down arrow button and select smz3.celestialrealm.net. Fill out your options, then when you click on Create game, you should see the following window:

  ![image](https://user-images.githubusercontent.com/63823784/209819364-6f2216f2-f24a-4554-b608-ac0e35f30254.png)

  Copy the game url, send it over to the people you want to play with, and they should be able to go to the Multiplayer tab, click join, and use that url to join you. Once all players have joined and updated their configs, the player who hosted should be able to Start the game. It'll generate the seed, send it out to all players and create the roms for everyone to play. Each player should then see buttons to launch the game. Simply open the game and start auto tracker, and you should be good to go!

  Interested in hosting your own multiplayer server? Going forward, we'll be including a separate download for the multiplayer server in these releases. You can view information [here](https://github.com/Vivelin/SMZ3Randomizer/blob/main/docs/ServerSetup.md) on how to host your own server!

  Enjoy, and try not to let tracker create too much of a divide between you and the other players! This is still pretty new, and not a lot of games have been completed. If you encounter any problems, please open an [Issue](https://github.com/Vivelin/SMZ3Randomizer/issues/new/choose) or post a message on betus's discord!

- **Extended MSU Support (with even extended-er support!)**

  As of this version of SMZ3, the MSU code now supports all of the same extended tracks as ALttPR! On the Super Metroid side, 10 additional tracks have been added as well, including special tracks for the 4 golden bosses, the baby Metroid, and more! For more information on all of the tracks supported, you can view this [Google Doc](https://docs.google.com/document/d/13h8Dh0Z56YFcsyhm89ICarZGY2fTBd7BY9CWeOpRU6Q/edit?usp=sharing) created by Phiggle!

- **Misc changes**
  - Updated .NET version. If needed, it should automatically install for you.
  - All possible hints are now added to the top of the spoiler log.
  - Item/location options for seed generation has been updated to be more reliable.
  - Updates to how tracker loads to be a bit faster (especially for multiworld).
  - Added a few new tracker responses, including one submitted by CPColin.
  - Fixed an issue with in game hints possibly including tracker voice altering commands.
  - Added chat detection for the new betus Tracker emote.

### Changes in 8.0.1

  - Fixed the Lower Norfair Torizo fight using the Ridley music instead of the normal Boss 1 theme

### Changes in 8.0.2

  - Updated the "crystal get" track to no longer loop to match ALttPR MSU support
  - Fixed an issue where falling from Ganon would cause sound to stop and eventually freeze the game
  - Fixed an issue where climbing GT with no MSU enabled would cause sound to stop and eventually freeze the game
  - Updated the code to assign the proper pendant/crystal themes for playing with vanilla music or with a non-extended MSUs
  - Temporarily removed the "Starting nerfed charge beam" due to UI issues and an issue where it could cause you to lose all your power bomb ammo.
  - Added extra logging for troubleshooting reported issues with generating seeds.

### Changes in 8.0.3

  - Readded the "starting nerfed charge beam" patch after fixing the assembly code to work with SMZ3.
  - Fixed an issue with the generator freezing when 0 hints were selected.
  - The easier wall jump and snap to morph ball hole patches are now optional and can be toggled on and off in the "Cas Patches."
  - Zora requiring you to find rupee pick ups is now optional as well the "Cas' Logic" section.
  - Added a few new tracker responses by CPColin and MattEqualsCoder.

## Changes in 7.0.0

- **In Game Hints**
 
  The telepathic tiles scattered throughout the dungeons and caves of Hyrule can now have hints which will tell you things like if a region is required or not, where late game items are, and what is at out of the way locations. There are 15 of these locations, and you can have as many hints as you want, with hints being duplicated if you don't have enough to populate all 15 locations.

 - **New Cas' Patches**
 
   Various different patches for both Zelda and Super Metroid have been added to make your experience all the more Cas'! Here are the current patches that are available:

    - Disable flashing effects in Zelda from things like Either.
    - Prevent scams by having the bottle merchant and Zora tell you of what they have. (Whether you have the rupees or not.)
    - Map the aim button in Super Metroid to any button.
    - Infinite Space Jump and Spin Jump Restart from the old Cas'troid patch are still there, but are now split into two separate options.
    - Speed keep to conserve your momentum in Metroid when moving horizontally and landing from a fall or jump.
    - Start the game with a nerfed charge beam to avoid soft locking to bosses.
    - Fast Metroid doors and elevators.
    - Fill up your ammo at save stations.
    - Update the Bozo door to close faster to prevent soft locking by getting stuck in the door.

  - **Customizable In Game Text**

    Blind, Ganon, Triforce Room, and some other in game text can be modified via a new profile config file game.yml similar to tracker responses. With this a lot of new lines have been added thanks to contributions from Phiggle and Fragger. As usual, there's an example file in the templates directory for you to see what all can be modified. Also, Sahasrahla and the Bomb Merchant now will use dungeon names designated in the other configs as well.

  - **Other Updates and Bug Fixes**
    - There are a lot of back end updates to merge the tracker and randomizer code to make management easier in the future.
    - The option to remove the grapple block at the entrance to Shaktool has been removed.
    - Fixed an issue in regards to tracker showing pyramid fairy or Sahasrahla's green pendant turn in as not in logic if you beat an order out of logic.
    - Updated Tracker to no longer say you are out of logic for two back-to-back locations if they are missing the exact same items.
    - Updated Tracker to no longer complain about being out of logic when all you're missing is keys when playing in Keysanity.
    - Some unnecessary text has been removed from the game, such as text when encountering the fairies.

### Changes in 7.0.1

Various minor things have been fixed or tweaked based on PinkKittyRose's stream.

  - Fixed blue crystals not showing up on the map.
  - Updated the plasma beam room to have the plasma beam as its vanilla item.
  - Fixed issue where tracker could comment about a location having a vanilla item when clearing that location and there is no vanilla item set for that location.
  - Updated tracker to not be able to undo defeating Ganon or Motherbrain when autotracked.
  - Tweaked the out of logic message frequency to have the sillier responses be a little more common since repeated out of logic messages are less likely to happen now.

### Changes in 7.0.2

This release mostly focuses on tweaks for undoing previously tracked things to minimize Tracker undoing things accidentally. In the future the ability to "redo" things will hopefully be added, but as it's not exactly trivial to develop, these changes are being implemented to hopefully make these situations less frequent.

  - The phrase "Hey tracker, undo" has being removed as it seemed to be the more common culprit to being picked up incorrectly by tracker. You'll want to say "Hey tracker, undo that".
  - Tracked actions which can be undone will now "expire" after a certain period of time, defaulting to 3 minutes. What this means is after that time period, tracker will no longer be able to undo it. The time duration can be customized in the options menu.
  - Various auto tracker actions were fixed to not be able to be undone.
  - A new logic config option has been added to logically require the Cane of Somaria to beat Kholdstare to make the experience more Cas' by basically never requiring you to have to beat the dungeon vanilla if you don't know how to bomb jump. Enabling this also prevents a current issue where tracker will show ice palace as in logic if you don't have Cane of Somaria until we get a system in place for tracker to better show that Ice Palace may not be beatable without bomb jumping.
  - When using the BizHawk emulator, auto tracking has been updated to verify that you're in a valid SMZ3 rom to prevent accidentally tracking items.
  - The lua connection has been updated to better handle situations where the lua script is restarted. Note that this may cause the connection to drop when in an emulator menu, but it should promptly reconnect upon closing the menu.
  - Made a couple minor additions to tracker responses.

### Changes in 7.0.3

This release addresses a couple items brought up by skennedysa.

  - Fixed an issue with using default Link and Samus sprites.
  - Added better error messaging when using the lua script runs into an issue loading the socket library.
  - Added lua script error troubleshooting details to the auto tracking help window.

### Changes in 7.0.4

This release has a couple small updates and fixes

- There's an update the BizHawk auto tracker script to work with the BizHawk shuffler better.
- "Hey tracker, I died" has been changed to "Hey tracker, I just died" to try to limit the number of times tracker mishears it.

Fragger also pulled in some new and updated Samus sprites:

- New Sprites:
  - Link 2 The Past (from LTTP but two of them)
  - Maria (from Castlevania: SOTN)
  - Maxim Kischine (from Castlevania: HoD)
  - MissingNo (from Pokemon)
  - Samus (AroAce)
  - Shante (from Shante 2)
  - Zelda 2 Link (from Zelda II: The Adventure of Link)
- Updated Sprites:
  - B.O.B.
  - Samus (Opposition)

### Changes in 7.0.5

This release implements a few accessibility options.

- Imported a couple patches from [VARIA](https://variabeta.pythonanywhere.com/)
  - Added the no flashing patch by kara (note that there is still some flashing, but it's definitely an improvement)
  - Added the disable screen shake patch by flo
- Added a new option to require either the space jump or gravity suit to get to blue Brinstar top (the Billy Mays room)
- Updated verbiage on the generator for the logic options to "Cas' Logic" and "Tricks and Advanced Logic" to be a bit more descriptive.

## Changes in 6.0.0

- **Map Updates**

  When bosses are accessible in either Zelda or Metroid, there is now an indicator on the map showing that you can fight the boss. In Zelda, this will show the reward for that dungeon. Locations that are accessible after beating a boss but can't currently be accessed are now shown with a empty square or circle. For keysanity, you will now see indicators on the map for Metroid keys you do not currently have.

  ![image](https://user-images.githubusercontent.com/63823784/187085475-8bede1d4-4022-4c43-a2b6-a56761ae36ac.png) ![image](https://user-images.githubusercontent.com/63823784/187087551-08d2a678-dad3-4a73-b98d-8ca6ffd49b7e.png)

- **Tracker profiles and easier config customization**

  The configuration for Tracker has had a major overhaul. Rather than the two large json files, they are now a series of yaml files to make things easier to find and edit. The default configs are now split into multiple profiles so that you can pick the types of responses you want. You can also create your own profiles if you want to add your own custom Tracker responses. While default profiles will be overridden with new updates, if you create your own profiles, they will be left alone. All enabled profiles will be merged. These profiles can be picked in the Randomizer Options before starting Tracker, and by default only the Sassy profile is enabled.

  ![image](https://user-images.githubusercontent.com/63823784/187085772-22ece1a4-3915-4fa1-9d9a-b6ee4b697dbd.png)

  To read more about how to add and edit your own profiles, you can view the documentation here: https://github.com/Vivelin/SMZ3Randomizer/tree/main/src/Randomizer.SMZ3.Tracking/Configuration/Yaml#readme

- **New Tracker UI Layouts**

  There are now multiple UI layouts that can be picked after Tracker has been started. There is a minimal layout that has just the essentials, an advanced layout that is more similar to what was in Tracker previously, and a keysanity layout. You can also create your own layout in a custom tracker profile by creating a ui.yaml config file. Along with that, you can include your own images for things like content in case you would rather have something other than the default or betus related content images. The details for how to do that are in the same page linked above.

  ![image](https://user-images.githubusercontent.com/63823784/187086010-669ae417-e22b-414b-8928-eef98090d501.png)

- **Plando Support**

  This version has full plando support. When creating a seed like normal, there will now be a yaml file created in the same folder as the rom which you can use as a starting point to edit. This yaml file can then be shared and uploaded into Tracker.

- **Additional Keysanity and Item Placement Options**

  Rather than simply turning on and off keysanity, there are now options for Zelda only keysanity and Metroid only keysanity. Along with this, there is now an option for where progression items can be placed. As these are new features, we can't fully say how they will feel in actual seeds, but hopefully they will offer some new twists to create unique seeds.

- **Shaktool no longer needs grapple beam!**

  That's right, it's even easier now to get to the best boy. There is a new option that will make it so that the grapple beam is no longer necessary for accessing Shaktool, meaning all you need is the Gravity Suit and either the Hi Jump Boots or Space Jump to be able to access Shaktool's room.

- Misc fixes and updates
  - Updates to GT guessing game. Tracker will now join in on guessing, count the number of items as you collect them, automatically close the game, and announce the winners.
  - There are new logic options to require the gravity suit to access waterway as well as require the space jump or speed booster to access the East Crateria sky item.
  - Cheating invalid items will no longer give you the sword and shield.
  - Regions, rooms, and locations can now have customizable out of logic text. This is used for things like preventing constant out of logic messages for GT when Metroid is not beatable and other things.
  - There is now an option to mute and unmute tracker if you would like her to stop interjecting for a bit. Simply say "Hey tracker, {mute/unmute} yourself".
  - Tracker will now comment if an item is at a vanilla location.
  - Aga, the bird, Super Metroid minibosses, Ganon, Mother Brain, and game completion should now be automatically tracked when autotracking is enabled.
  - You can now say "Hey tracker, I beat the game" to pause the timer at the end.
  - The baby ship sprite should no longer cause a crash.
  - This includes a new set of Metroid maps from Fragger which are now the defaults. The old maps are still accessible via the dropdown and voice command.
  - You should no longer have to say dash for certain location names
  - The left pit spring ball option no longer also requires hi-jump
  - There are fixes for the logic regarding to north west checks of Maridia
  - Game modes are now copied by default with settings strings
  - You will now be prompted to re-authenticate into Twitch if the previous token has expired

- New Sprites
  - Link: B.O.B
  - Samus: Sans (from Undertale), Mini Samus, Samus (Clocktoberfest, Fusion, Opposition), SNES Controller, Combat Armor (a version of the Zero Suit), Mario (Dream Team), and Richter Belmot
  - Ships: Castlevania Stairs, USS Enterprise, Phazon Ship, X-Wing, Ship 1-5, N64, Egg Prison, and Bowser Ship
    
### Changes in 6.0.1
- **Fixed** issue with tracker trying to auto track the bird a second time when switching from Super Metroid to Zelda
- Additional tracker line added

This is a casual version of the [original SMZ3 randomizer](https://github.com/tewtal/SMZ3Randomizer).

## Changes in 5.3.0

- **Easier wall jumping**!

  Thanks to [a patch][EasierWJ] by Benox50, wall jumping is now a lot easier! This also includes a patch called [Collision Clearance][Celeste] by the same author that is supposed to make going into tiny holes as morph ball easier. Have fun!

[EasierWJ]: https://metroidconstruction.com/resource.php?id=545
[Celeste]: https://metroidconstruction.com/resource.php?id=544

- **Cheats**

  When auto tracker is connected, you can now enable cheats to assist you in the game. These cheats can't be undone, so be mindful of that! To enable cheats, say "Hey tracker, enable cheats".

  Current cheats:
  - Give yourself any item (including big 20s!) - "Hey tracker, give me [item]"
  - Fill your health, magic, bombs, arrows, rupees, missiles, super missiles, or power bombs - "Hey tracker, fill my [health/magic/etc]"
  - Kill yourself (for those tactical resets) - "Hey tracker, [kill me/give me a tactical reset]"

- Dark room map updates
  - Thanks to Fragger, the Palace of Darkness basement map now shows enemies that spawn in the rooms
  - Added Misery Mire and Turtle Rock dark room maps (these may be replaced by Fragger in the future)

- Misc fixes and updates
  - Added some new Super Metroid sprites from VARIA
  - Some locations have new alternative names or have their names fixed
  - Various new tracker comments and sass by @Vivelin 
  - Fixed a bug that could cause auto tracker to mark Agahnim as cleared incorrectly
  - Updated the default tracker item window size to show all items
  - Fixed an issue where the launcher would crash if no default application is set for sfcs in Windows
  - The screw attack location is now accessible via screw attack when soft lock prevention is turned on

## Changes in 5.2.0

- **Wall jump difficulty option**

   Thanks to the efforts of PinkKittyRose going through and documenting all of the wall jumps in the game, there is now an option to set the difficulty of expected wall jumps. While there may be some subtle changes in the logic from these changes, the default of Medium should be pretty similar to the previous SMZ3 logic. You can set it to a lower difficulty to avoid having to wall jump as much or even increase it if you're comfortable making tricky jumps that were previously considered out of logic.
   
- **Better Keysanity Support**

   Following PinkKittyRoses's keysanity playthrough, multiple fixes were implemented to address things that came up during her run.

  - The map window for keysanity now only factors in keys for showing what's in logic.
  - The "take a look at this" should now require the map for keysanity to avoid spoiling location rewards.
  - All locations in dungeons should now be counted as treasure for keysanity mode.

- **Better Race Support**

   Made changes for races to address issues, add flexibility, and make it easier to copy race settings.

  - Fixed issue with the race flag pulling from last used settings rather than the generated rom.
  - Removed Tracker corrections if hints/spoilers are disabled to prevent accidental spoilers.
  - Dungeon boss item tracking has been removed to also prevent accidental spoilers.
  - Disabled/race spoiler logs no longer contain location information.
  - Added additional settings for disabling spoiler logs, hints, and spoilers individually when generating a seed without the race mode turned on for more flexibility.
  - There is now a checkbox when pasting a import settings string to copy the seed number and all race settings from the import string to make joining races easier.

- Misc Updates
  - Fixed non green pendant rewards not triggering the pendant dip message.
  - Additional tracker comment updates.
  - Fixed issue with tracker showing medallion dungeons in logic when you haven't marked them.
  - Fixed one location in Skull Woods not auto tracking

### Changes in 5.2.1

- **Progression Log**
  - Following a run, in the menu you can right click to generate a progression log that shows the history of your navigation through the game, item collection, and defeated bosses. **Note that you will need to make sure that you save the tracker session before closing out!** Auto tracking is recommended for the most detailed log.
  ![image](https://user-images.githubusercontent.com/63823784/178116518-379e541b-068a-421d-aa49-8a1044b7eeee.png)
  ![image](https://user-images.githubusercontent.com/63823784/178116545-06d86b19-59c8-4465-b032-b52cb58eaada.png)

- **Dark Room Maps**
  - Various dark rooms now have maps added to assist with navigation through them without the lamp. You can pull them up manually through the dropdown, by saying "hey tracker show me [location name]", or by entering a dark room with auto tracking enabled and saying "Hey tracker, [it's dark in here/I can't see/show me this dark room map]". When leaving the dark room, you can say "Hey tracker, [I can see now/I can see clearly now/it's no longer dark/I'm out of the dark room/stop showing me the dark room map]"
  - The following are the dark rooms currently supported
    - Eastern Palace Side
    - Eastern Palace Back
    - Death Mountain Old Man Cave
    - Dark Palace Maze
    - Dark Palace Basement (image may not be final)
    - Hyrule Castle Escape (image may not be final)

- Misc Updates
  - Added a fallback method to copy seed numbers and settings strings 
  - Fixed issues with copying config strings and not starting the run immediately
  - Fixed an issue with Blind's item not tracking properly
  - Made a fix that will hopefully avoid USB2SNES connections from reading the memory too early and marking 
  - Added voice commands to pause/resume/reset by using the voice commands "Hey tracker, [pause/stop/resume/start/reset] the timer"
  - Auto tracker will now reset the time when starting the game at the ship
  - Added more tracker voice lines

## Changes in 5.1.0

- **USB2SNES/QUSB2SNES Auto Tracking Support**

   Auto tracking now has support for USB2SNES, which opens up the compatibility of auto tracking with emulators such as RetroArch and SNES hardware that support USB2SNES/QUSB2SNES. Now instead of simply enabling auto tracking, you select whether you want it to run via Lua Script for snes9x-rr and bizhawk out of box support or USB2SNES support.

   ![image](https://user-images.githubusercontent.com/63823784/173093561-c99f98a3-4d48-4495-83f7-54ea6f083cf9.png)

   For those interested, if you go to Tools -> Options in the main SMZ3 Randomizer window and open up the Tracker options, you can specify which auto tracker connector to run by default as well as turn off the automatic map updating.

   ![image](https://user-images.githubusercontent.com/63823784/173093986-f74366eb-b693-456e-9797-a0ccc81c3166.png)

   Also wanted to give thanks to PinkKittyRose for helping test this QUSB2SNES support

- **(Somewhat) Automatic Dungeon Reward Marking**

   If auto tracking is enabled, if you open up the light or dark world map and tell tracker "Hey tracker, <please/would you please> <look at this/look here/record this/log this/take a look at this> <shit/crap>", she will mark the dungeon rewards for you. Note that she currently can't differentiate between red and blue pendants. This functionality will be expanded in the coming weeks to include medallions and visible traysure.

- **Bug fixes and minor improvements**
   - Fixed an issue with Tracker not realizing a Twitch poll was opened
   - Added additional trick and state tracking comments for tracker
   - As per usual, additional sass and comments for Tracker by Fragger and myself

### Changes in 5.1.1
- **Fixed** issue with ice breaker comment going off too early

### Changes in 5.1.2
- **Fixed** (hopefully) an issue with the auto tracker lua script potentially being overwhelmed with memory requests, causing it slowdown
- **Fixed** an issue where in certain situations cleared locations checked could happen for the wrong game if slowdown is happening
- Temporarily removed fake flippers detection as it could be detected incorrectly in certain locations where you can swim for a tiny bit before drowning, such as near Lake Hylia island
- Laser bridge logic should now work with the mirror shield

### Changes in 5.1.3
- **Fixed** (again) a rare issue with tracking some Zelda locations when switching to Super Metroid
- **Fixed** the auto tracker thinking you were in Ganon's Tower when you entered Mimic Cave
- **Fixed** an issue with incorrectly identifying diver down sometimes
- Added updated Shaktool sprites by Fragger
- Added Kirby and Dread Samus SM sprites by Wandering✝Spider from VARIA
- Various tracker comments updates by Fragger and myself

### Changes in 5.1.4
- Added an optional logic config for requiring the ice beam for old tourian launchpad
- Added a non-green pendant dungeon reward for auto tracking to use
- Added an option to disable tracker's speech (this will be updated to have more options in the future)
- Updated the application to work on both 32bit and 64bit systems
- Updated the installer to now install all required dependencies
- Added new content images from betus's new sub badges
- Added second set of voice lines for falling from the GT moldorm

## Changes in 5.0.0

- **Auto Tracking Support**

   By connecting tracker to your emulator via a Lua script, Tracker is now able to automatically clear locations and track items for you. Tracker can also detect what's going on in the game and will comment on various situations. Auto tracking was tested in Snes9x-rr (recommended) and Bizhawk 2.8 (BSNES Core Only), though older versions or other emulators may work. To enable auto tracking or view information on running it, there is an icon in the status bar of the tracker window:

   ![image](https://user-images.githubusercontent.com/63823784/171869075-d0cfabc0-565a-4634-b813-367e97fee676.png)

   By default these scripts are in your local app data/SMZ3CasRandomizer directory (which you can view by clicking on Show Auto Tracker Scripts Folder), but you can specify a different directory in the settings as well as enable the auto tracker by default.

- **Tracker Map Updates**

   You can now update the displayed map by saying "Hey tracker, please show me <Hyrule/Zebes/Brinstar/Light World/Everything/etc.>" to update the map displayed. If auto tracking is enabled and the setting is turned on (it's on by default), the map will automatically update to reflect your location.

- **The Duck is Now Deployed** ![duck](https://user-images.githubusercontent.com/63823784/171874227-6755fe4e-5d66-4abe-8d51-18f7105dd03d.png)

   By popular demand, you can now track duck to add it to the tracker.

- **Both Shaktool and Love Shak sprites by Fragger have been added**

- **Bug fixes and minor improvements**
   - Fixed some of the articles for some items that start with vowels
   - The GT guessing game now requires "Hey Tracker" before it
   - Various additional tracker lines by Fragger and MattEqualsCoder
   - Mimic cave now shows up on the map

### Changes in 5.0.1
- **Fixed** tracker Twitch chat connection issues to prevent crashes

   If tracker cannot connect to Twitch chat or connection drops, Tracker will now state that she is not connected and will prompt you to save and restart the tracker (you should not need to restart the entire app). The code was also updated to do any GT guessing game commands if not currently connected, and there were updates to better handle issues with not being able to send messages to the Twitch chat.

- **Fixed** an issue with tracker thinking she opened a poll when one was already opened (hopefully 🤞)

- **Updated** tracker with new lines from both Fragger and MattEqualsCoder

## Changes in 4.2

- **Reworked "_Hey tracker, clear \<dungeon\>_"** (#89, #90)

   This command is now separate from clearing other areas, and will clear all items, including those that might be out of logic, and set the remaining treasure counter to zero. If any out-of-logic checks were cleared, Tracker will let you know about it, but still clear it.

- **Tracker will no longer tolerate being interrupted too many times** (#98)
- **Various new lines and songs for Tracker** (#87, #100)

   This includes some changes suggested by community member Constantine.

- **Bug fixes and minor improvements** (#91, #100)
   - Fixed IP binding issue in options window
   - Fixed Tracker not connecting to Twitch properly when starting a second time (#92)
   - Improved pronunciation by using display name rather than username (#93)
   - Improved idle detection by ignoring low confidence 'noise'

### Changes in 4.2.1

- Fixed multiple dungeon-clearing issues (#104)
  - Fixed clearing Hyrule Castle not marking it as completed (#102)
  - Fixed force-tracking everything in a dungeon room not decreasing the treasure counter (#103)
- Added "_Hey tracker, what's in \<area\>?_" (#105)
- Fixed Tracker being interrupted too many times causing some weird stuff to happen (hopefully) (#106)

### Changes in 4.2.2

- Added Ganon's Tower Big Key Guessing Game support when using Twitch integration (#115)
- Various bug fixes and polish (#113)
  - **Removed interruptions**
    It was funny for a little bit but didn't turn out the way I wanted, and at this stage it's not worth trying to get it to work well
  - Fixed bullshit being considered progression
  - Fixed an error when asking for hints about an item that can't be found (anymore)
  - Fixed chat integration not working when entering a channel URL instead of just the username in the **Twitch Channel** field
  - Improved username pronunciation by treating underscores as spaces
  - Fixed "_You need the Fire Rod before you can get the Fire Rod_" hints

### Changes in 4.2.3

- Fixed Tracker incorrectly claiming you were out of logic when tracking all items in certain areas (e.g. Hype Cave after doing Aga)
- Added more recognized greetings and a bunch more songs for Tracker
- Added final version of King Graham sprite by Fragger

### Changes in 4.2.4

- Added ability to call out to the Twitch API for polls. **Note**: This requires reauthenticating into Twitch for additional permissions.
- Used the above poll functionality to create polls to confirm from chat if content should be increased

### Changes in 4.2.5

- Added commands for either quick killing or killing a boss without taking damage which will increase content automatically. This applies to all bosses currently, even if some of them don't make sense. It's up to the streamer/player to use it when it makes most sense. If you don't, maybe chat won't be so kind the next time it's polled to increase content! The commands are as follows:
   - Hey tracker, [I beat/I defeated/I beat off/I killed] [BossName] [Without getting hit/without taking damage/and didn't get hit/and didn't take damage/in one cycle]
   - Hey tracker, [I one cycled/I quick killed] [BossName]
- There's a new optional logic config for requiring the spring ball and hi jump boots to access the left sand pit in Maridia.
- Increased frequency of polling viewers for content updates from 25% of the time to 33% of the time.

### Changes in 4.2.6

- **Added** the new Rash sprite from Fragger
- **Added** new tracker responses from Fragger and myself

## Changes in 4.1
- Added basic Twitch chat integration! #79 
  - Use the **Log in with Twitch** option in the Options window to log in with Twitch and allow Tracker to access Twitch chat.

    > **⚠️ Please note that the OAuth token field should be kept confidential! ⚠️**
    Don't show the **Options** window on stream, don't tell anyone what this value is, and don't send someone your **options.json** file as it contains this token. It can be used to impersonate you in Twitch chat.

  - In this version, Tracker will greet people that say hey in chat, but this will be expanded upon in the future.
- Fixes to prevent issues with playing seeds that don't match the requested settings. #78
  - After generating a seed, it will now throw an error popup if it doesn't match the user settings
  - Spazer and tunics no longer show up as potential early items as they are not technically progression items

- Findings and suggestions from streams #80 
  - New jokes, songs and other tracker.json changes.
  - "_Hey tracker, I died_" now tracks deaths.
### Changes in 4.1.1

- Tracker now knows who you are.
- Added an option to toggle chat greeting and an optional time limit.
- Fixed some Tracker pronunciation issues.

## Changes in 4.0
- **Added** the ability to make and share more custom seeds #18 #72 
  - Most progression items can now be updated to appear early.
  - You are now able to customize any location to force them to be progression, junk, or a specific item.
  - Upon clicking the button to generate the rom, any potentially unwinnable seeds should be detected. Depending on the options, you can try to click the button again as it may have just been bad luck, but if it persists you may have to change your settings.
  - After generating seeds, you can now now right click on in the rom list to "Copy Randomizer Settings String" which can be pasted at the bottom of the Generate custom game window. This will copy all seed generation setting. All customization settings like sprites are not copied.
- **Added** multiple new tricks #76 
  - Mockball #58 
  - Navigating dark rooms outside of Hyrule Tower with only a sword #61 
  - Light World South Fake Flippers #75 
-  Misc. changes and bug fixes #71 #73 
   - Added more phrases for Tracker.
   - When asking what's at a location, tracker will now tell you whether it's good or not right away.
   - Tracker will respect the spoiler threshold setting when tracking all items in an area.
   - Fixed space jump timing when enabling Super Cas'Troid.

## Changes in 3.4
- **Added** the ability to customize the randomizer generation by toggling logic options and tricks. #54   
- **Added** custom ship sprite options based on the patches from [VARIA customizer](https://randommetroidsolver.pythonanywhere.com/customizer). #55 
-  Misc. changes. #53 
   - **Improved** the hint system, including the new ability to ask "give me a hint" to get a suggestion of areas that have progression items.
   - **Fixed** Green and Pink Brinstar being considered accessible with just speed booster.
     - Terminator was already not in logic, but the areas beyond still were.
   - The Tracker window no longer resizes automatically.
   - Tracking everything in an area now mentions every item that was picked up.

### Changes in 3.4.1
- **Fixed** Tracker spouting nonsense when clearing multiple times in an area

### Changes in 3.4.2
- "_Give me a hint_" no longer gives hints for an area that has an item you already have (e.g. early super missile)


## Changes in 3.3
- **Changed** fire rod logic for dark rooms to be consistent with logic in ALttPR. #44 
- **Improved** performance related to progression checks. #42 
- **Added** the ability for Tracker to recognize which specific items are missing. #45 
  - Currently, this is used when asking for hints about a specific item, and as a bit of sass when tracking an item that's currently out of logic.
  - This might be expanded in the future (e.g. "_Hey tracker, what's missing for Go Mode?_")
- **Added** a few some interesting sprites from ALttPR and VARIA randomizer.
- **Added** an experimental Space Jump patch that relaxes the timings a bit and makes it easier to use (_Super Cas’troid_ option in the randomizer).
-  Misc. changes. #46 
   - **Added** a status bar item that shows the last recognized phrase.
   - **Added** item name to hints when asking about an already cleared location.
   - **Fixed** Tracker saying "But you already have that" when tracking a boss after clearing the item that the boss drops.
   - **Fixed** undoing "Clear X treasures" only adding back 1 treasure.

### Changes in 3.3.1 #47 
- **Added** a menu speed option.
- **Added** item location options for Pegasus Boots and Space Jump.
- **Restored** Ganon's Tower logic requiring all main Metroid bosses to be defeated.
- **Changed** the recognized text status bar display to have a maximum length.

### Changes in 3.3.2 #48 
- **Updated** _Super Cas’troid_ option to include the Space Jump Restart (Respin) patch.

### Changes in 3.3.3
- **Fixed** Sanctuary Heart sass.
- **Added** Chair sprite by \_aitchFactor.
- **Added** ability for Tracker to comment on the junk/progression status of items tracked from a specific location, e.g. after saying _"track Hammer from Shaktool"_.


## Changes in 3.2
This is mostly a maintenance release before I go on a brief hiatus.

- **Fixed** a crash on starting Tracker with a `tracker.json` from an older version;
  - It's still a good idea to re-apply any customizations manually after updating as you might miss out on new features otherwise.
- **Changed** bosses into a separate entity for consistency. Both Metroid and Zelda bosses can now be tracked/marked as defeated using the same commands;
  - Defeating a Zelda boss still tracks the item they drop; Metroid bosses do not.
- **Removed** "Love" as alternative for "Heart Container" because it was causing multiple mistracks;
- Minor other config changes.


## Changes in 3.1
The main window now features a list of ROMs you've generated, with the ability to start playing or view the spoiler log from there. Contributed by @MattEqualsCoder.

- [_Hotfix 3.1.1_] **Fixed** dungeon keysanity logic in hint commands being inverted;
- **Added** hints & spoilers command, (#37);
  - _"Hey tracker, where's {item}?"_
  - _"Hey tracker, what's at {location}?"_
- **Added** Agahnim's Tower and Mother Brain to tracker;
- **Added** better location names to the location & map windows;
  - This mostly affects location names from Super Metroid; new names were taking mostly from the wiki and the Super Metroid speedrunning community;
  - These names can now be customized through `locations.json` in `%LocalAppData%\SMZ3CasRandomizer`;
- Various bugfixes and improvements based on stream feedback (#35);

## Changes in 3.0.1
- **Fixed** triple digit counters;
- **Fixed** duplicate Content counters;
- **Changed** Tracker to use the counter multiplier in voice responses (e.g. 10 missiles instead of 2);
- **Changed** marking a location as having bullshit (or any other "nothing" item) to clear the location instead;
- **Changed** _"Doesn't get you anywhere though"_ to trigger less often;
- **Added** Hyrule Castle and Ganon's Tower to Tracker;
- **Added** voice command for updating the number of items owned, e.g. _"Hey tracker, I have 85 missiles"_;
  - Note: This might get removed in the future if it fucks with the voice recognition too much.
- **Added** deaths counter to Tracker;

## Changes in 3.0
- **Added** item & location tracker with a map view contributed by @MattEqualsCoder 
  - Generate a new game or hit **Play**, then press **Start Tracker** to try it out!

For more details, see the in-game voice commands reference by starting Tracker, then going to **View** and selecting **Help,** or read the [pre-release patch notes](https://github.com/Vivelin/SMZ3Randomizer/releases/tag/v3.0-tracker-22).

Items, bosses and Tracker responses can be customized by editing the `%LocalAppData%\SMZ3CasRandomizer\tracker.json` file. **Please note that this file may be overwritten by updates**, so make sure to back up your changes before you update!

## Changes in 2.1.2.2
- **Added** Shaktool sprites by Pneumatic from the [VARIA randomizer](https://randommetroidsolver.pythonanywhere.com/customizer).

## Changes in 2.1.2.1
- **Fixed** crash when generating seeds after updating from v2.0. Please note that some settings may be lost.

If you're already on a 2.1 or higher, this version can be skipped.

## Changes in 2.1.2
- **Removed** some soundtracks from the Shuffle All option that didn't work;
- **Fixed** bad checksum warnings that appear when running with snes9x;
- **Improved** seed option to allow any input, not just numbers;
- Window size and the collapsed/expanded state of the groups are now saved on exit and restored on startup.

## Changes in 2.1.1
- **Added** option to configure the folder where new seeds will be saved to.
  - Please note that for the built-in MSU-1 support to work, the MSU files need to be on the same drive. If you move the seeds to a new location, you'll need to move the MSU files as well.
- **Added** heart color and low health beeping options.

## Changes in 2.1
- **Fixed**  crash when playing without a custom music pack selected. Extended soundtrack support is now an option and can only be turned on if a music pack is selected;
- **Added** dungeon music shuffle option, inspired by the bug in the previous version;
- **Added** Peg World ("Hammer Pegs") item pool option.