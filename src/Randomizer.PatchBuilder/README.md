# Patch Builder

The Patch Builder project is used to easily build and test changes to the Assembly. It allows you to use ASAR and other tools to build the patches, create a test rom with the patches applied, and even automatically launch the rom.

## Pre-Requisites

Add the following pre-requisites to the alttp_sm_combo_randomizer_rom\resources folder:

1. ASAR 1.81 - https://github.com/RPGHacker/asar
    - For Windows, you can download the latest release. For Linux, you will need to download the 1.81 source code and build it.
2. Lunar IPS (Windows) - https://www.romhacking.net/utilities/240/
3. Floating IPS (Linux) - https://www.romhacking.net/utilities/1040/
4. Super Metroid Rom File
    - Name it as sm.sfc
    - Optionally include z3.sfc in the same location for generating the test rom

## Initialization

When you run the Patch Builder project for the first time, it will generate a patch-config.yml file which you will edit to toggle flags for what you want the Patch Builder to do, edit the rom that will be generated, and more.

## Patch Config Settings

- **PatchFlags** - Enables/disables functionality of the Patch Builder project
    - **CreatePatches** - Run ASAR & the IPS tool to generate the new IPS patches
    - **CopyPatchesToProject** - Copies the IPS patches to the Randomizer.SMZ3 project to be bundled with the application
    - **GenerateTestRom** - If a test rom should be generated to play
    - **AssignMsu** - If the first MSU in the Msu Paths Patch Options field should be applied to the test rom
    - **LaunchTestRom** - If the test rom should be launched
- **EnvironmentSettings** - Updates paths that will be used. All of these fields are optional.
    - **MetroidRomPath** - Path to the Metroid rom file. Will look for sm.sfc in the alttp_sm_combo_randomizer_rom\resources if not provided.
    - **MetroidRomPath** - Path to the Metroid rom file. Will look for z3.sfc in the alttp_sm_combo_randomizer_rom\resources if not provided.
    - **TestRomFileName** - Name of the test rom file (without the sfc file extension). Will use test-rom if not provided.
    - **PatchBuildScriptPath** - Path to the script file to execute to compile the assembly. Will use alttp_sm_combo_randomizer_rom/build.bat or alttp_sm_combo_randomizer_rom/build.sh depending on operating system if not provided.
    - **LaunchApplication** - Application to launch for playing the test rom. If not provided, it will use the default application associated with the sfc file extension.
    - **LaunchArguments** - Arguments to pass to the launch application. If nothing is provided, it will just pass the rom file. If needed, %rom% can be specified to substitute out the path to the rom file. If %rom% is not included, it will be appended to the end of the other arguments.
- **PatchOptions** - Settings for how to patch the test rom. Most of these you can pull from your RandomizerOptions json file.
    - **SelectedSprites** - If you want to use a specific sprite, include the path to the sprite and set IsDefault to false.
    - **MsuPaths** - This is an array of MSU paths. The Patch Builder will use the first MSU in the array if AssignMsu is set to true.

## Launch Application & Arguments
 - **snes9x** - If you want to use a specific version of snes9x, simply include set the LaunchApplication as the full path to the snes9x exe file
 - **RetroArch** - For RetroArch, you will need to specify both LaunchApplication and LaunchArguments. LaunchApplication will be set to the RetroArch exe file. Set the LaunchArguments to:
    ```
    -L "<full path to the desired core file>"
    ```
    The rom path will automatically be appended to the end. For Linux, you may need to use a bash script to execute RetroArch in a new terminal.