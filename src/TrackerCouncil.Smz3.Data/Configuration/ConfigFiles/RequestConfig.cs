using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using static TrackerCouncil.Smz3.Data.Configuration.ConfigTypes.SchrodingersString;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Config file for additional request information
/// </summary>
[Description("Config file for custom tracker prompts that she will listen to and her responses to them.\r\n" +
             "Note that all prompts will automatically have \"Hey tracker\" added before them.")]
public class RequestConfig : List<BasicVoiceRequest>, IMergeable<BasicVoiceRequest>, IConfigFile<RequestConfig>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RequestConfig() : base()
    {
    }

    /// <summary>
    /// Returns default request information
    /// </summary>
    /// <returns></returns>
    public static RequestConfig Default()
    {
        return new RequestConfig();
    }

    public static object Example()
    {
        return new RequestConfig()
        {
            new()
            {
                Phrases = new() { "first message tracker will listen for", "second message tracker will listen for" },
                Response = new("First response tracker will respond with when saying \"Hey tracker, first message tracker will listen for\" or \"Hey tracker, second message tracker will listen for\"", new Possibility("Another message tracker can respond with", 0.1)),
            },
            new()
            {
                Phrases = new() { "another request" },
                Response = new("another response"),
            },
        };
    }
}
