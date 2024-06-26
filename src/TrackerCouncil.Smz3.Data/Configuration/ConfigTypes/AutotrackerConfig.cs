using System.Collections.Generic;
using TrackerCouncil.Data.Configuration;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Provides the phrases for chat integration.
/// </summary>
public class AutoTrackerConfig : IMergeable<AutoTrackerConfig>
{
    /// <summary>
    /// Gets the phrases to respond with when connected to to emulator.
    /// </summary>
    public SchrodingersString? WhenConnected { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when disconnected from the emulator.
    /// </summary>
    public SchrodingersString? WhenDisconnected { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the game is started.
    /// </summary>
    public SchrodingersString? GameStarted { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the game is started in a multiplayer game.
    /// <c>{0}</c> is a placeholder for the number of players.
    /// <c>{1}</c> is a placeholder for a random player name.
    /// </summary>
    public SchrodingersString? GameStartedMultiplayer { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when nearing KAD
    /// </summary>
    public SchrodingersString? NearKraidsAwfulSon { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when nearing shaktool
    /// </summary>
    public SchrodingersString? NearShaktool { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when nearing crocomire
    /// </summary>
    public SchrodingersString? NearCrocomire { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when at crumble shaft
    /// </summary>
    public SchrodingersString? AtCrumbleShaft { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when falling down the pit from moldorm
    /// </summary>
    public SchrodingersString? FallFromMoldorm { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when falling down the pit from moldorm in GT
    /// </summary>
    public SchrodingersString? FallFromGTMoldorm { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when falling down the pit from moldorm
    /// </summary>
    public SchrodingersString? FallFromGanon { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when performing Hera Pot tech
    /// </summary>
    public SchrodingersString? HeraPot { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when performing Ice Breaker tech
    /// </summary>
    public SchrodingersString? IceBreaker { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when performing Specky Clip
    /// </summary>
    public SchrodingersString? SpeckyClip { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when performing Diver Down tech
    /// </summary>
    public SchrodingersString? DiverDown { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when entering Hyrule Castle tower
    /// </summary>
    public SchrodingersString? EnterHyruleCastleTower { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when entering a pendant dungeon
    /// </summary>
    public SchrodingersString? EnterPendantDungeon { get; init; }

    /// <summary>
    /// Entered GT without all crystals
    /// </summary>
    public SchrodingersString? EnteredGTEarly { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when swimming without flippers
    /// </summary>
    public SchrodingersString? FakeFlippers { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when performing mockball
    /// </summary>
    public SchrodingersString? MockBall { get; init; }

    /// <summary>
    /// Skipping spore spawn
    /// </summary>
    public SchrodingersString? SkipSporeSpawn { get; init; }

    /// <summary>
    /// Seeing Ridley's face entrance
    /// </summary>
    public SchrodingersString? RidleyFace { get; init; }

    /// <summary>
    /// Player asked auto tracker to look at something, but there was nothing
    /// </summary>
    public SchrodingersString? LookedAtNothing { get; init; }

    /// <summary>
    /// Light world only has pendants
    /// </summary>
    public SchrodingersString? LightWorldAllCrystals { get; init; }

    /// <summary>
    /// Misery Mire and Turtle Rock have pendants
    /// </summary>
    public SchrodingersString? DarkWorldNoMedallions { get; init; }

    /// <summary>
    /// Big Key is the first item found by the player in GT
    /// </summary>
    public SchrodingersString? GTKeyIsItemOne { get; init; }

    /// <summary>
    /// Big key was the second to seventh item found by the player in GT
    /// </summary>
    public SchrodingersString? GTKeyIsItemTwoToSeven { get; init; }

    /// <summary>
    /// Big key was the eighth to fifteenth item found by the player in GT
    /// </summary>
    public SchrodingersString? GTKeyIsItemEightToFifteen { get; init; }

    /// <summary>
    /// Big key was the sixteenth to twenty first item found by the player in GT
    /// </summary>
    public SchrodingersString? GTKeyIsItemSixteenToTwentyOne { get; init; }

    /// <summary>
    /// Big key was the twenty second item found by the player in GT
    /// </summary>
    public SchrodingersString? GTKeyIsItemTwentyTwo { get; init; }

    /// <summary>
    /// Responses based on what number the GT key was. Tracker will use the responses for the largest number less
    /// than the number of the GT key.
    /// </summary>
    public Dictionary<int, SchrodingersString?>? GTKeyResponses { get; init; }

    /// <summary>
    /// Auto tracker detected switching to SMZ3
    /// </summary>
    public SchrodingersString? SwitchedToSMZ3Rom { get; init; }

    /// <summary>
    /// Responses for changing to a game other than SMZ3. The key represents the hash of the
    /// rom that is being switched to. For any hashes that aren't found, the responses for
    /// SwitchedToOtherRom["Unknown"] is used.
    /// </summary>
    public Dictionary<string, SchrodingersString?>? SwitchedToOtherRom { get; init; }
}
