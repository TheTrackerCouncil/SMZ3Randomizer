using System.IO;
using System.Linq;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.IpsPatches;

public static class IpsPatch
{
    public static Stream GetStream(string name)
    {
#if DEBUG
        var path = Path.Combine(Directories.SolutionPath, "src", "TrackerCouncil.Smz3.SeedGenerator", "FileData", "IpsPatches", name);
        return File.OpenRead(path);
#else
        var type = typeof(IpsPatch);
        return type.Assembly.GetManifestResourceStream(type, name) ?? throw new FileNotFoundException($"Not able to load IPS patch {name}");
#endif
    }

    /// <summary>
    /// Gets a stream for the IPS patch that enables custom ship sprite support.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream CustomShip() => GetStream("custom_ship.ips");

    /// <summary>
    /// Gets a stream for the IPS patch that enables MSU-1 support.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream MsuSupport() => GetStream("msu1-v6.ips");

    /// <summary>
    /// Gets a stream for the IPS patch that contains the base SMZ3 ROM patches.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream Smz3() => GetStream("zsm.ips");

    /// <summary>
    /// Gets a stream for the IPS patch the contains the Respin patch.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream Respin() => GetStream("spinjumprestart.ips");

    /// <summary>
    /// Gets a stream for the IPS patch the contains the nerfed charge patch.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream NerfedCharge() => GetStream("nerfed_charge.ips");

    /// <summary>
    /// Gets a stream for the IPS patch the contains the refill before saving patch.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream RefillAtSaveStation() => GetStream("refill_before_save.ips");

    /// <summary>
    /// Gets a stream for the IPS patch the contains the fast Super Metroid doors patch.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream FastDoors() => GetStream("fast_doors.ips");

    /// <summary>
    /// Gets a stream for the IPS patch the contains the fast Super Metroid elevators patch.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream FastElevators() => GetStream("elevators_speed.ips");

    /// <summary>
    /// Gets a stream for the IPS patch the contains the patch to customize the aim buttons.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream AimAnyButton() => GetStream("AimAnyButton.ips");

    /// <summary>
    /// Gets a stream for the IPS patch the contains the preserve momentum / speedkeep patch.
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream SpeedKeep() => GetStream("rando_speed.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to prevent flashing in Super Metroid
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream DisableMetroidFlashing() => GetStream("noflashing.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to prevent screen shaking in Super Metroid
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream DisableMetroidScreenShake() => GetStream("disable_screen_shake.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to make it easier to wall jump
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream EasierWallJumps() => GetStream("EasierWJ.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to make it easier to get into morph passages
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream SnapMorph() => GetStream("Celeste.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to allow you to run without holding the run button
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream AutoRun() => GetStream("AutoRun.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to allow you to quick toggle SM items with the item cancel button
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream ItemCancelToggle() => GetStream("QuickToggle.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to allow you to hold the item cancel button to fire supers/powerbombs
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream ItemCancelHoldFire() => GetStream("HoldFire.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to have a single aim button as aim up and replacing aim down with
    /// a quick morph
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream UnifiedAim() => GetStream("UnifiedAim.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to add platforms around the Maridia sand pits
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream SandPitPlatforms() => GetStream("SandPitPlatforms.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to remove the super missile block from the old Tourian launchpad
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream CrateriaLaunchpadExitNoSuperBlock() => GetStream("climb_supers.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to change the bomb block in the Crateria moat to shoot blocks
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream CrateriaMoatNoBombBlock() => GetStream("moat.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to change the bomb block in Green Brin mockball hall to a shoot block
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream GreenBrinMockballHallNoBombBlock() => GetStream("early_super_bridge.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to make the speed booster blocks in pink brinstar Dachora room not respawn
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream PinkBrinDachoraSpeedboosterBlockNoRespawn() => GetStream("dachora.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to remove the bomb blocks to the Pink Brin save rooms
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream PinkBrinSaveEntranceNoBombBlock() => GetStream("spospo_save.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to change the bomb block in the Pink Brinstar room under the Sidehopper room
    /// to a shoot block
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream PinkBrinSidehopperPitRoomNoBombBlock() => GetStream("mission_impossible.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to add platforms to the Red Brinstar Red Tower to allow you to get up easier
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream RedBrinRedTowerPlatforms() => GetStream("red_tower.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to change the bomb block in front of the Spazer room to a shoot block
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream RedBrinSpazerNoBombBlock() => GetStream("spazer.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to change the bomb block to the Kraid save room to a shoot block
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream KraidSaveNoBombBlock() => GetStream("kraid_save.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to change the bomb block in when leaving the Hi Jump item room to a shoot block
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream NorfairHiJumpExitNoBombBlock() => GetStream("high_jump.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to make it easier to get into Upper Norfair east without the hi jump
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream NorfairCathedralEntranceNovaBoostPlatformBlock() => GetStream("nova_boost_platform.ips");

    /// <summary>
    /// Gets a stream for the IPS patch to allow you to get to upper Maridia from the top of Red Brin
    /// </summary>
    /// <returns>A new stream that contains the IPS patch.</returns>
    public static Stream MaridiaTopEntrance() => GetStream("maridia_entrance.ips");


}
