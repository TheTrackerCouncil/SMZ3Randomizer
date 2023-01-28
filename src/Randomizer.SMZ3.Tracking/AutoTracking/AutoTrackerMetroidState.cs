namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Used to retrieve certain states based on the memory in Metroid
    /// Seee https://jathys.zophar.net/supermetroid/kejardon/RAMMap.txt for details on the memory
    /// </summary>
    public class AutoTrackerMetroidState
    {
        private readonly EmulatorMemoryData _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        public AutoTrackerMetroidState(EmulatorMemoryData data)
        {
            _data = data;
        }

        /// <summary>
        /// The overall room number the player is in
        /// </summary>
        public int CurrentRoom => _data.ReadUInt8(0x7E079B - 0x7E0750);

        /// <summary>
        /// The region room number the player is in
        /// </summary>
        public int CurrentRoomInRegion => _data.ReadUInt8(0x7E079D - 0x7E0750);

        /// <summary>
        /// The current region the player is in
        /// </summary>
        public int CurrentRegion => _data.ReadUInt8(0x7E079F - 0x7E0750);

        /// <summary>
        /// The amount of energy/health
        /// </summary>
        public int Energy => _data.ReadUInt16(0x7E09C2 - 0x7E0750);

        /// <summary>
        /// The amount currently in reserve tanks
        /// </summary>
        public int ReserveTanks => _data.ReadUInt16(0x7E09D6 - 0x7E0750);

        /// <summary>
        /// The max of health
        /// </summary>
        public int MaxEnergy => _data.ReadUInt16(0x7E09C4 - 0x7E0750);

        /// <summary>
        /// The max in reserve tanks
        /// </summary>
        public int MaxReserveTanks => _data.ReadUInt16(0x7E09D4 - 0x7E0750);

        /// <summary>
        /// Samus's X Location
        /// </summary>
        public int SamusX => _data.ReadUInt16(0x7E0AF6 - 0x7E0750);

        /// <summary>
        /// Samus's Y Location
        /// </summary>
        public int SamusY => _data.ReadUInt16(0x7E0AFA - 0x7E0750);

        /// <summary>
        /// Samus's current super missile count
        /// </summary>
        public int SuperMissiles => _data.ReadUInt8(0x7E09CA - 0x7E0750);

        /// <summary>
        /// Samus's max super missile count
        /// </summary>
        public int MaxSuperMissiles => _data.ReadUInt8(0x7E09CC - 0x7E0750);

        /// <summary>
        /// Samus's current missile count
        /// </summary>
        public int Missiles => _data.ReadUInt8(0x7E09C6 - 0x7E0750);

        /// <summary>
        /// Samus's max missile count
        /// </summary>
        public int MaxMissiles => _data.ReadUInt8(0x7E09C8 - 0x7E0750);

        /// <summary>
        /// Samus's current power bomb count
        /// </summary>
        public int PowerBombs => _data.ReadUInt8(0x7E09CE - 0x7E0750);

        /// <summary>
        /// Samus's max power bomb count
        /// </summary>
        public int MaxPowerBombs => _data.ReadUInt8(0x7E09D0 - 0x7E0750);

        public bool IsSamusInArea(int minX, int maxX, int minY, int maxY)
        {
            return SamusX >= minX && SamusX <= maxX && SamusY >= minY && SamusY <= maxY;
        }

        /// <summary>
        /// Checks to make sure that the state is valid and fully loaded. There's a period upon first booting up that
        /// all of these are 0s, but some of the memory in the location data can be screwy.
        /// </summary>
        public bool IsValid => CurrentRoom != 0 || CurrentRegion != 0 || CurrentRoomInRegion != 0 || Energy != 0 ||
                               SamusX != 0 || SamusY != 0;

        /// <summary>
        /// Prints debug data for the state
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"CurrentRoom: {CurrentRoom} | CurrentRoomInRegion: {CurrentRoomInRegion} | CurrentRegion: {CurrentRegion} | Health: {Energy},{ReserveTanks} | X,Y {SamusX},{SamusY}";
        }
    }
}
