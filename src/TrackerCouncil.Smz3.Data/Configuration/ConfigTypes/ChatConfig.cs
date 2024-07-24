using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Provides the phrases for chat integration.
/// </summary>
public class ChatConfig : IMergeable<ChatConfig>
{
    /// <summary>
    /// Gets a collection of greetings that tracker recognizes and responds
    /// to.
    /// </summary>
    public List<string>? RecognizedGreetings { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when connected to chat.
    /// </summary>
    public SchrodingersString? WhenConnected { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when reconnected to chat.
    /// </summary>
    public SchrodingersString? WhenReconnected { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when disconnected to chat.
    /// </summary>
    public SchrodingersString? WhenDisconnected { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when disconnected to chat.
    /// </summary>
    public SchrodingersString? NoConnection { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when greeted by someone in chat.
    /// <c>{0}</c> is a placeholder for the display name of the person in
    /// chat to respond to.
    /// </summary>
    public SchrodingersString? GreetingResponses { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when greeting someone for the
    /// second time in chat.
    /// <c>{0}</c> is a placeholder for the display name of the person in
    /// chat to respond to.
    /// </summary>
    public SchrodingersString? GreetedTwice { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when greeted by the broadcaster.
    /// <c>{0}</c> is a placeholder for the display name of the person in
    /// chat to respond to.
    /// </summary>
    public SchrodingersString? GreetedChannel { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when starting the GT big key
    /// guessing game.
    /// </summary>
    public SchrodingersString? StartedGuessingGame { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when closing guesses for the GT big
    /// key guessing game.
    /// </summary>
    public SchrodingersString? ClosedGuessingGame { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when a moderator closed guesses for
    /// the GT big key guessing game.
    /// <c>{0}</c> is a placeholder for the name of the moderator that
    /// triggered the close.
    /// </summary>
    public SchrodingersString? ModeratorClosedGuessingGame { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the guessing game is closed
    /// subsequent times.
    /// </summary>
    public SchrodingersString? ClosedGuessingGameWhileClosed { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the guessing game is closed
    /// before it was started.
    /// </summary>
    public SchrodingersString? ClosedGuessingGameBeforeStarting { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when a moderator closed the
    /// guessing game when it has already been closed.
    /// <c>{0}</c> is a placeholder for the name of the moderator that
    /// triggered the close.
    /// </summary>
    public SchrodingersString? ModeratorClosedGuessingGameWhileClosed { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when a moderator closed the
    /// guessing game when it hasn't even started yet.
    /// <c>{0}</c> is a placeholder for the name of the moderator that
    /// triggered the close.
    /// </summary>
    public SchrodingersString? ModeratorClosedGuessingGameBeforeStarting { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the guessing game has
    /// concluded.
    /// <c>{0}</c> is a placeholder for the correct number. <c>{1}</c> is a
    /// placeholder for the names of the winners.
    /// </summary>
    public SchrodingersString? DeclareGuessingGameWinners { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the guessing game has
    /// concluded.
    /// <c>{0}</c> is a placeholder for the correct number. <c>{1}</c> is a placeholder for the guessed number,
    /// <c>{2}</c> is a placeholder for the names of the winners.
    /// </summary>
    public SchrodingersString? DeclareGuessingGameClosestButNotOverWinner { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the guessing game has
    /// concluded and nobody won.
    /// <c>{0}</c> is a placeholder for the correct number.
    /// </summary>
    public SchrodingersString? NobodyWonGuessingGame { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when Tracker submits her own guess
    /// for the guessing game.
    /// <c>{0}</c> is a placeholder for the number that Tracker guessed.
    /// </summary>
    public SchrodingersString? TrackerGuess { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when Tracker is among the list of
    /// winners that were just announced.
    /// <c>{0}</c> is a placeholder for the correct number.
    /// </summary>
    public SchrodingersString? TrackerGuessWon { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when Tracker is the only winner of
    /// the guessing game.
    /// <c>{0}</c> is a placeholder for the correct number.
    /// </summary>
    public SchrodingersString? TrackerGuessOnlyWinner { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the chest that Tracker guessed
    /// was opened and did not contain the big key.
    /// </summary>
    public SchrodingersString? TrackerGuessFailed { get; init; } // TODO: implement

    /// <summary>
    /// Gets a dictionary that contains usernames and their replacement for
    /// text-to-speech pronunciation purposes.
    /// </summary>
    public Dictionary<string, string>? UserNamePronunciation { get; init; }

    /// <summary>
    /// Gets the phrases for when asking the chat if content should be
    /// increased or not
    /// </summary>
    public SchrodingersString? AskChatAboutContent { get; init; }

    /// <summary>
    /// Gets the phrases for when chat decided to increase content
    /// </summary>
    public SchrodingersString? AskChatAboutContentYes { get; init; }

    /// <summary>
    /// Gets the phrases for when chat decided not to increase content
    /// </summary>
    public SchrodingersString? AskChatAboutContentNo { get; init; }

    /// <summary>
    /// Gets the phrases for when the poll is complete
    /// </summary>
    public SchrodingersString? PollComplete { get; init; }

    /// <summary>
    /// Gets the phrases for when the poll has votes, but it was terminated before being finished
    /// </summary>
    public SchrodingersString? PollCompleteTerminated { get; init; }

    /// <summary>
    /// Gets the phrases for when the poll is opened
    /// </summary>
    public SchrodingersString? PollOpened { get; init; }

    /// <summary>
    /// Gets the phrases for when the poll outcome could not be determined
    /// </summary>
    public SchrodingersString? PollError { get; init; }

    /// <summary>
    /// Gets the phrases for when the poll was terminated before any results came in
    /// </summary>
    public SchrodingersString? PollErrorTerminated { get; init; }
}
