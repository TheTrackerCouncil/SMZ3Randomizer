using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Provides the phrases for chat integration.
    /// </summary>
    public class ChatConfig
    {
        /// <summary>
        /// Gets a collection of greetings that tracker recognizes and responds
        /// to.
        /// </summary>
        public IReadOnlyCollection<string> RecognizedGreetings { get; init; }
            = new List<string>();

        /// <summary>
        /// Gets the phrases to respond with when connected to chat.
        /// </summary>
        public SchrodingersString WhenConnected { get; init; }
            = new("Hello chat.");

        /// <summary>
        /// Gets the phrases to respond with when greeting by someone in chat.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the user name of the person in chat
        /// to respond to.
        /// </remarks>
        public SchrodingersString GreetingResponses { get; init; }
            = new("Hey {0}");

        /// <summary>
        /// Gets a dictionary that contains usernames and their replacement for
        /// text-to-speech pronunciation purposes.
        /// </summary>
        public IReadOnlyDictionary<string, string> UserNamePronunciation { get; init; }
            = new Dictionary<string, string>();
    }
}
