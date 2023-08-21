using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles;

public class MsuConfig : IMergeable<MsuConfig>, IConfigFile<MsuConfig>
{
    /// <summary>
    /// Names for various tracks when asking "what's the current song for"
    /// </summary>
    public Dictionary<int, SchrodingersString> TrackLocations = new Dictionary<int, SchrodingersString>()
    {
        { 2, new SchrodingersString("light world", "hyrule field") },
        { 4, new SchrodingersString("bunny") },
        { 5, new SchrodingersString("lost woods") },
        { 7, new SchrodingersString("kakariko", "kakariko village", "link's house") },
        { 8, new SchrodingersString("mirror") },
        { 9, new SchrodingersString("dark world") },
        { 10, new SchrodingersString("pedestal pull", "master sword pedestal", "pedestal") },
        { 11, new SchrodingersString("zelda game over") },
        { 12, new SchrodingersString("guards") },
        { 13, new SchrodingersString("dark death mountain") },
        { 14, new SchrodingersString("minigame") },
        { 15, new SchrodingersString("dark woods") },
        { 16, new SchrodingersString("hyrule castle") },
        { 17, new SchrodingersString("pendant dungeon") },
        { 18, new SchrodingersString("cave 1", "mini moldorm cave", "superbunny cave", "paradox cave", "spike cave", "spiral cave", "Kakariko well", "bumper cave", "hype cave", "link's uncle", "checkerboard cave", "hookshot cave", "Mire shed", "hammer pegs") },
        { 19, new SchrodingersString("boss victory", "Boss fanfare") },
        { 20, new SchrodingersString("sanctuary") },
        { 21, new SchrodingersString("zelda boss battle") },
        { 22, new SchrodingersString("crystal dungeon") },
        { 23, new SchrodingersString("shop") },
        { 24, new SchrodingersString("cave 2", "lost woods hideout", "king's tomb", "dam", "swamp ruins", "waterfall fairy", "magic bat", "graveyard ledge", "sahasrahla", "pyramid fair", "aginah", "cave 45", "cave number 45") },
        { 26, new SchrodingersString("crystal retrieved", "crystal get") },
        { 27, new SchrodingersString("fairy", "great fairy", "ice rod cave") },
        { 28, new SchrodingersString("agahnims floor") },
        { 29, new SchrodingersString("ganon reveal", "ganon bat") },
        { 30, new SchrodingersString("ganons message", "ganon intro") },
        { 31, new SchrodingersString("ganon battle", "ganon fight") },
        { 32, new SchrodingersString("triforce room") },
        { 33, new SchrodingersString("epilogue", "zelda epilogue", "zelda credits") },
        { 35, new SchrodingersString("eastern palace") },
        { 36, new SchrodingersString("desert palace") },
        { 37, new SchrodingersString("agahnims tower") },
        { 38, new SchrodingersString("swamp palace") },
        { 39, new SchrodingersString("palace of darkness", "dark palace") },
        { 40, new SchrodingersString("misery mire") },
        { 41, new SchrodingersString("skull woods") },
        { 42, new SchrodingersString("ice palace") },
        { 43, new SchrodingersString("tower of hera") },
        { 44, new SchrodingersString("thieves town") },
        { 45, new SchrodingersString("turtle rock") },
        { 46, new SchrodingersString("ganons tower") },
        { 47, new SchrodingersString("armos knights") },
        { 48, new SchrodingersString("lanmolas") },
        { 49, new SchrodingersString("agahnim 1") },
        { 50, new SchrodingersString("arrghus") },
        { 51, new SchrodingersString("helmasaur king") },
        { 52, new SchrodingersString("vitreous") },
        { 53, new SchrodingersString("mothula") },
        { 54, new SchrodingersString("kholdstare") },
        { 55, new SchrodingersString("moldorm") },
        { 56, new SchrodingersString("blind") },
        { 57, new SchrodingersString("trinexx") },
        { 58, new SchrodingersString("agahnim 2") },
        { 59, new SchrodingersString("ganons tower climb", "g t climb") },
        { 60, new SchrodingersString("light world 2") },
        { 61, new SchrodingersString("dark world 2") },
        { 99, new SchrodingersString("s m z 3 credits") },
        { 101, new SchrodingersString("samus fanfare") },
        { 102, new SchrodingersString("item acquired") },
        { 103, new SchrodingersString("item room", "elevator room") },
        { 104, new SchrodingersString("metroid opening with intro") },
        { 105, new SchrodingersString("metroid opening without intro") },
        { 106, new SchrodingersString("crateria landing with thunder", "crateria landing") },
        { 107, new SchrodingersString("crateria landing without thunder") },
        { 108, new SchrodingersString("crateria space pirates appear", "blue brinstar", "crateria 1", "space pirates appear") },
        { 109, new SchrodingersString("golden statues", "golden four") },
        { 110, new SchrodingersString("samus aran theme", "crateria 2") },
        { 111, new SchrodingersString("green brinstar", "pink brinstar") },
        { 112, new SchrodingersString("red brinstar", "kraid's lair") },
        { 113, new SchrodingersString("upper norfair") },
        { 114, new SchrodingersString("lower norfair") },
        { 115, new SchrodingersString("inner maridia") },
        { 116, new SchrodingersString("outer maridia") },
        { 117, new SchrodingersString("tourian") },
        { 118, new SchrodingersString("mother brain") },
        { 119, new SchrodingersString("big boss battle 1", "bozo", "bomb torizo", "golden torizo") },
        { 120, new SchrodingersString("evacuation", "escape") },
        { 121, new SchrodingersString("chozo statue awakens", "mysterious statue chamber") },
        { 122, new SchrodingersString("big boss battle 2", "crocomire") },
        { 123, new SchrodingersString("tension", "boss incoming", "hostile incoming") },
        { 124, new SchrodingersString("plant miniboss", "spore spawn", "botwoon") },
        { 126, new SchrodingersString("wrecked ship powered off") },
        { 127, new SchrodingersString("wrecked ship powered on") },
        { 128, new SchrodingersString("theme of super metroid") },
        { 129, new SchrodingersString("death cry", "samus death") },
        { 130, new SchrodingersString("metroid credits") },
        { 131, new SchrodingersString("kraid incoming") },
        { 132, new SchrodingersString("kraid battle", "kraid fight") },
        { 133, new SchrodingersString("phantoon incoming") },
        { 134, new SchrodingersString("phantoon battle", "phantoon fight") },
        { 135, new SchrodingersString("draygon battle", "draygon fight") },
        { 136, new SchrodingersString("ridley battle", "ridley fight") },
        { 137, new SchrodingersString("baby incoming") },
        { 138, new SchrodingersString("the baby") },
        { 139, new SchrodingersString("hyper beam") },
        { 140, new SchrodingersString("metroid game over", "super metroid game over") },
    };

    /// <summary>
    /// Gets the phrases for what song is playing
    /// </summary>
    /// <remarks>
    /// <c>{0}</c> is a placeholder for the song details
    /// </remarks>
    public SchrodingersString? CurrentSong { get; init; } = new("That's {0}", "That song is {0}");

    /// <summary>
    /// Gets the phrases for what msu the current song is from
    /// </summary>
    /// <remarks>
    /// <c>{0}</c> is a placeholder for the msu details
    /// </remarks>
    public SchrodingersString? CurrentMsu { get; init; } = new("The song is from the MSU pack {0}", "That song is from {0}");

    /// <summary>
    /// Gets the phrases for not being able to determine the playing song
    /// </summary>
    public SchrodingersString? UnknownSong { get; init; } = new("Sorry, I could not get the song details");

    /// <summary>
    /// Returns default response information
    /// </summary>
    /// <returns></returns>
    public static MsuConfig Default()
    {
        return new MsuConfig();
    }
}
