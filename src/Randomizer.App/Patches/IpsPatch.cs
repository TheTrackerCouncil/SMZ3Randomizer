using System;
using System.IO;

namespace Randomizer.App.Patches
{
    internal static class IpsPatch
    {
        public static Stream GetStream(string name)
        {
            var type = typeof(IpsPatch);
            return type.Assembly.GetManifestResourceStream(type, name);
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
    }
}
