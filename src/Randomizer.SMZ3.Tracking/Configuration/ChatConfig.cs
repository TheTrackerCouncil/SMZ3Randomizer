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
        /// Gets the phrases to respond with when greeted by someone in chat.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the display name of the person in
        /// chat to respond to.
        /// </remarks>
        public SchrodingersString GreetingResponses { get; init; }
            = new("Hey {0}");

        /// <summary>
        /// Gets the phrases to respond with when greeting someone for the
        /// second time in chat.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the display name of the person in
        /// chat to respond to.
        /// </remarks>
        public SchrodingersString? GreetedTwice { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when greeted by the broadcaster.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the display name of the person in
        /// chat to respond to.
        /// </remarks>
        public SchrodingersString? GreetedChannel { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when starting the GT big key
        /// guessing game.
        /// </summary>
        public SchrodingersString StartedGuessingGame { get; init; }
            = new("The floor is now open for guesses.");

        /// <summary>
        /// Gets the phrases to respond with when closing guesses for the GT big
        /// key guessing game.
        /// </summary>
        public SchrodingersString ClosedGuessingGame { get; init; }
            = new("The floor is now closed for guesses.");

        /// <summary>
        /// Gets the phrases to respond with when a moderator closed guesses for
        /// the GT big key guessing game.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the moderator that
        /// triggered the close.
        /// </remarks>
        public SchrodingersString ModeratorClosedGuessingGame { get; init; }
            = new("{0} closed the floor for guesses.");

        /// <summary>
        /// Gets the phrases to respond with when the guessing game has
        /// concluded.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the correct number.
        /// <c>{1}</c> is a placeholder for the names of the winners.
        /// </remarks>
        public SchrodingersString DeclareGuessingGameWinners { get; init; }
            = new("The winners who guessed number {0} are {1}.");

        /// <summary>
        /// Gets the phrases to respond with when the guessing game has
        /// concluded and nobody won.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the correct number.
        /// </remarks>
        public SchrodingersString NobodyWonGuessingGame { get; init; }
            = new("Nobody guessed {0}.");

        /// <summary>
        /// Gets a dictionary that contains usernames and their replacement for
        /// text-to-speech pronunciation purposes.
        /// </summary>
        public IReadOnlyDictionary<string, string> UserNamePronunciation { get; init; }
            = new Dictionary<string, string>();
    }
}
