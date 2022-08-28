namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Used to retrieve certain states based on the memory in Zelda
    /// See http://alttp.run/hacking/index.php?title=RAM:_Bank_0x7E:_Page_0x00 for details on the memory
    /// These memory address values are the offset from 0x7E0000
    /// </summary>
    public class AutoTrackerZeldaState
    {
        private readonly EmulatorMemoryData _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        public AutoTrackerZeldaState(EmulatorMemoryData data)
        {
            _data = data;
        }

        /// <summary>
        /// The current room the player is in
        /// </summary>
        public int CurrentRoom => ReadUInt16(0xA0);

        /// <summary>
        /// The previous room the player was in
        /// </summary>
        public int PreviousRoom => ReadUInt16(0xA2);

        /// <summary>
        /// The state of the game (Overworld, Dungeon, etc.)
        /// </summary>
        public int State => ReadUInt8(0x10);

        /// <summary>
        /// The secondary state value
        /// </summary>
        public int Substate => ReadUInt8(0x11);

        /// <summary>
        /// The player's Y location
        /// </summary>
        public int LinkY => ReadUInt16(0x20);

        /// <summary>
        /// The player's X Location
        /// </summary>
        public int LinkX => ReadUInt16(0x22);

        /// <summary>
        /// What the player is currently doing
        /// </summary>
        public int LinkState => ReadUInt8(0x5D);

        /// <summary>
        /// Value used to determine if the player is in the light or dark world
        /// Apparently this is used for other calculations as well, so need to be a bit careful
        /// Transitioning from Super Metroid also seems to break this until you go through a portal
        /// </summary>
        public int OverworldValue => ReadUInt8(0x7B);

        /// <summary>
        /// True if Link is on the bottom half of the current room
        /// </summary>
        public bool IsOnBottomHalfOfRoom => ReadUInt8(0xAA) == 2;

        /// <summary>
        /// True if Link is on the right half of the current room
        /// </summary>
        public bool IsOnRightHalfOfRoom => ReadUInt8(0xA9) == 1;

        /// <summary>
        /// The overworld screen that the player is on
        /// </summary>
        public int OverworldScreen => ReadUInt16(0x8A);

        /// <summary>
        /// Reads a specific block of memory
        /// </summary>
        /// <param name="address">The address offset from 0x7E0000</param>
        /// <returns></returns>
        public int ReadUInt8(int address) => _data.ReadUInt8(address);

        /// <summary>
        /// Reads a specific block of memory
        /// </summary>
        /// <param name="address">The address offset from 0x7E0000</param>
        /// <returns></returns>
        public int ReadUInt16(int address) => _data.ReadUInt16(address);

        /// <summary>
        /// Get debug string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var vertical = IsOnBottomHalfOfRoom ? "Bottom" : "Top";
            var horizontal = IsOnRightHalfOfRoom ? "Right" : "Left";
            return $"Room: {PreviousRoom}->{CurrentRoom} ({vertical}{horizontal}) | State: {State}/{Substate} | X,Y: {LinkX},{LinkY} | LinkState: {LinkState} | OW Screen: {OverworldScreen}";
        }
    }
}
