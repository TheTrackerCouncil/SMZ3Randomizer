namespace Randomizer.SMZ3.Tracking.AutoTracking
{
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
}
