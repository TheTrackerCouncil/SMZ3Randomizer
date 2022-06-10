using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool ShouldSend(Game currentGame, bool hasStartedGame)
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

    }

    /// <summary>
    /// Which game(s) the message should be sent to the emulator in
    /// </summary>
    public enum Game
    {
        /// <summary>
        /// Send if the player has not started the game
        /// </summary>
        Neither,

        /// <summary>
        /// Send if the player is in Super Metroid
        /// </summary>
        SM,

        /// <summary>
        /// Send if the player is in Zelda
        /// </summary>
        Zelda,

        /// <summary>
        /// Send if the player is in either game
        /// </summary>
        Both
    }

    /// <summary>
    /// The type of action
    /// </summary>
    public enum EmulatorActionType
    {
        ReadBlock
    }

    /// <summary>
    /// The type of memory
    /// </summary>
    public enum MemoryDomain
    {
        /// <summary>
        /// SNES Memory
        /// </summary>
        WRAM,

        /// <summary>
        /// Cartridge Memory / Save File (AKA SRAM)
        /// </summary>
        CartRAM,

        /// <summary>
        /// Game data saved on cartridge
        /// </summary>
        CartROM
    }
}
