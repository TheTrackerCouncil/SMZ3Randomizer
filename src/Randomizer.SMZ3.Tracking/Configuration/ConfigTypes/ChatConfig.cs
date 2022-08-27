using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
{
    /// <summary>
    /// Provides the phrases for chat integration.
    /// </summary>
    public class ChatConfig : IMergeable<ChatConfig>
    {
        /// <summary>
        /// Gets a collection of greetings that tracker recognizes and responds
        /// to.
        /// </summary>
        public List<string> RecognizedGreetings { get; init; }
            = new List<string>();

        /// <summary>
        /// Gets the phrases to respond with when connected to chat.
        /// </summary>
        public SchrodingersString WhenConnected { get; init; }
            = new("Hello chat.");

        /// <summary>
        /// Gets the phrases to respond with when disconnected to chat.
        /// </summary>
        public SchrodingersString WhenDisconnected { get; init; }
            = new("Error with Twitch chat connection. Please save and restart tracker.");

        /// <summary>
        /// Gets the phrases to respond with when disconnected to chat.
        /// </summary>
        public SchrodingersString NoConnection { get; init; }
            = new("I'm not currently connected to Twitch chat.");

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
        /// Gets the phrases to respond with when the guessing game is closed
        /// subsequent times.
        /// </summary>
        public SchrodingersString ClosedGuessingGameWhileClosed { get; init; }
            = new("The floor is already closed.");

        /// <summary>
        /// Gets the phrases to respond with when the guessing game is closed
        /// before it was started.
        /// </summary>
        public SchrodingersString ClosedGuessingGameBeforeStarting { get; init; }
            = new("But we haven't even started yet.");

        /// <summary>
        /// Gets the phrases to respond with when a moderator closed the
        /// guessing game when it has already been closed.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the moderator that
        /// triggered the close.
        /// </remarks>
        public SchrodingersString ModeratorClosedGuessingGameWhileClosed { get; init; }
            = new("The floor is already closed, {0}.");

        /// <summary>
        /// Gets the phrases to respond with when a moderator closed the
        /// guessing game when it hasn't even started yet.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the moderator that
        /// triggered the close.
        /// </remarks>
        public SchrodingersString ModeratorClosedGuessingGameBeforeStarting { get; init; }
            = new("But we haven't even started yet, {0}.");

        /// <summary>
        /// Gets the phrases to respond with when the guessing game has
        /// concluded.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the correct number. <c>{1}</c> is a
        /// placeholder for the names of the winners.
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
        /// Gets the phrases to respond with when Tracker submits her own guess
        /// for the guessing game.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the number that Tracker guessed.
        /// </remarks>
        public SchrodingersString TrackerGuess { get; init; }
            = new("My guess is {0}.", "I'm joining with {0}.", "I have a feeling it'll be {0}");

        /// <summary>
        /// Gets the phrases to respond with when Tracker is among the list of
        /// winners that were just announced.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the correct number.
        /// </remarks>
        public SchrodingersString TrackerGuessWon { get; init; }
            = new("Hey, that's me.");

        /// <summary>
        /// Gets the phrases to respond with when Tracker is the only winner of
        /// the guessing game.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the correct number.
        /// </remarks>
        public SchrodingersString TrackerGuessOnlyWinner { get; init; }
            = new("Am I the only one who guessed {0}? I swear I wasn't cheating.");

        /// <summary>
        /// Gets the phrases to respond with when the chest that Tracker guessed
        /// was opened and did not contain the big key.
        /// </summary>
        public SchrodingersString? TrackerGuessFailed { get; init; } // TODO: implement

        /// <summary>
        /// Gets a dictionary that contains usernames and their replacement for
        /// text-to-speech pronunciation purposes.
        /// </summary>
        public Dictionary<string, string> UserNamePronunciation { get; init; }
            = new Dictionary<string, string>()
            {
                ["Vivelin"] = "vihvelin",
                ["MattEqualsCoder"] = "matt equals coder",
                ["Axnollouse"] = "Fragger"
            };

        /// <summary>
        /// Gets the phrases for when asking the chat if content should be
        /// increased or not
        /// </summary>
        public SchrodingersString AskChatAboutContent { get; init; }
            = new("Hmm. I'm not so sure about that. Let's ask the professionals in chat if that was some hashtag content.");

        /// <summary>
        /// Gets the phrases for when chat decided to increase content
        /// </summary>
        public SchrodingersString AskChatAboutContentYes { get; init; }
            = new("It's your lucky day. Chat has confirmed that was some hashtag content.");

        /// <summary>
        /// Gets the phrases for when chat decided not to increase content
        /// </summary>
        public SchrodingersString AskChatAboutContentNo { get; init; }
            = new("I'm glad I asked. The chat has denied your request to increase your content levels.");

        /// <summary>
        /// Gets the phrases for when the poll is complete
        /// </summary>
        public SchrodingersString PollComplete { get; init; }
            = new("And the results are now in.");

        /// <summary>
        /// Gets the phrases for when the poll is opened
        /// </summary>
        public SchrodingersString PollOpened { get; init; }
            = new("I have opened a poll for {0} seconds.");

        /// <summary>
        /// Gets the phrases for when the poll outcome could not be determined
        /// </summary>
        public SchrodingersString PollError { get; init; }
            = new("Sorry, I was unable to get the poll results.");
    }
}
