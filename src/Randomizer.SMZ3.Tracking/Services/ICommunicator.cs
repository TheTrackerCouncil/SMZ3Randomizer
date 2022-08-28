namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Defines a mechanism to communicate with the player.
    /// </summary>
    public interface ICommunicator
    {
        /// <summary>
        /// Communicates the specified text to the player.
        /// </summary>
        /// <param name="text">
        /// The plain text or SSML representation of the text to communicate.
        /// </param>
        void Say(string text);

        /// <summary>
        /// Communicates the specified text to the player and blocks the calling
        /// thread until the text has been fully communicated to the player.
        /// </summary>
        /// <param name="text">
        /// The plain text or SSML representation of the text to communicate.
        /// </param>
        void SayWait(string text);

        /// <summary>
        /// When overriden in an implementing class, it will enable communicating
        /// </summary>
        void Enable();

        /// <summary>
        /// When overriden in an implementing class, it will disable communicating
        /// </summary>
        void Disable();

        /// <summary>
        /// When overridden in an implementing class, aborts all ongoing
        /// communications.
        /// </summary>
        public void Abort() { }

        /// <summary>
        /// When overridden in an implementing class, uses an alternate voice
        /// when communicating with the player.
        /// </summary>
        public void UseAlternateVoice() { }

        /// <summary>
        /// When overridden in an implementing class, increases the
        /// communication rate.
        /// </summary>
        public void SpeedUp() { }

        /// <summary>
        /// When overridden in an implementing class, decreases the
        /// communication rate.
        /// </summary>
        public void SlowDown() { }
    }
}
