using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

namespace TrackerCouncil.Smz3.Data.Options;

/// <summary>
/// Images for a single reaction
/// </summary>
public class TrackerSpeechReactionImages
{
    /// <summary>
    /// Image for when tracker is not talking
    /// </summary>
    public required string IdleImage { get; set; }

    /// <summary>
    /// Image for when tracker is saying something
    /// </summary>
    public required string TalkingImage { get; set; }
}

/// <summary>
/// A package of different sets of reaction images for Tracker
/// </summary>
public class TrackerSpeechImagePack
{
    /// <summary>
    /// The name of the pack
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The default reaction images for the speech image pack
    /// </summary>
    public required TrackerSpeechReactionImages Default { get; set; }

    /// <summary>
    /// A dictionary of all of the different reaction types for this pack
    /// </summary>
    public required Dictionary<string, TrackerSpeechReactionImages> Reactions { get; set; }

    public required TrackerProfileConfig? ProfileConfig { get; set; }

    /// <summary>
    /// Gets the reaction images for a given reaction type. Will return the default reaction type if not specified
    /// or the requested reaction type is not present in this pack.
    /// </summary>
    /// <param name="reactionName">The name of the reaction</param>
    /// <returns>The appropriate images to use for the reaction</returns>
    public TrackerSpeechReactionImages GetReactionImages(string? reactionName = null)
    {
        if (reactionName == null)
        {
            return Default;
        }
        return Reactions.TryGetValue(reactionName.ToLower(), out var reaction) ? reaction : Default;
    }
}
