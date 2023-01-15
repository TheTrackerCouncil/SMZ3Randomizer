using System;
using System.Collections.Generic;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Class used for communicating between the emulator and tracker
    /// </summary>
    public class EmulatorAction
    {
        /// <summary>
        /// The action to be done by the emulator (read/write/etc)
        /// </summary>
        public EmulatorActionType Type { get; set; }

        /// <summary>
        /// The starting memory address
        /// </summary>
        public int Address { get; set; }

        /// <summary>
        /// The number of bytes to capture from the emulator
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Values for writing
        /// </summary>
        public ICollection<byte>? WriteValues { get; set; }

        /// <summary>
        /// The type of memory to read or modify (WRAM, CARTRAM, CARTROM)
        /// </summary>
        public MemoryDomain Domain { get; set; }

        /// <summary>
        /// The game this message is for
        /// </summary>
        public Game Game { get; set; } = Game.Both;

        /// <summary>
        /// Action to perform when getting a response for this from the emulator
        /// </summary>
        public Action<EmulatorAction>? Action { get; set; }

        /// <summary>
        /// The previous data collected for this action
        /// </summary>
        public EmulatorMemoryData? PreviousData { get; protected set; }

        /// <summary>
        /// The latest data collected for this action
        /// </summary>
        public EmulatorMemoryData? CurrentData { get; protected set; }

        /// <summary>
        /// Update the stored data and invoke the action
        /// </summary>
        /// <param name="data">The data collected from the emulator</param>
        public void Invoke(EmulatorMemoryData data)
        {
            PreviousData = CurrentData;
            CurrentData = data;
            Action?.Invoke(this);
        }

        /// <summary>
        /// If this message should be sent based on the game the player is currently in
        /// </summary>
        /// <param name="currentGame">The game the player is currently in</param>
        /// <param name="hasStartedGame">If the player has actually started the game</param>
        /// <returns>True if the message should be sent.</returns>
        public bool ShouldProcess(Game currentGame, bool hasStartedGame)
        {
            return (!hasStartedGame && Game == Game.Neither) || (hasStartedGame && Game != Game.Neither && (Game == Game.Both || Game == currentGame));
        }

        /// <summary>
        /// Checks if the data has changed between the previous and current collections
        /// </summary>
        /// <returns>True if the data has changed, false otherwise</returns>
        public bool HasDataChanged()
        {
            return CurrentData != null && !CurrentData.Equals(PreviousData);
        }

        /// <summary>
        /// Cached set of locations for this action
        /// </summary>
        public ICollection<Location>? Locations { get; set; }

        /// <summary>
        /// Clears both the previous and current data sets
        /// </summary>
        public void ClearData()
        {
            PreviousData = null;
            CurrentData = null;
        }

    }

}
