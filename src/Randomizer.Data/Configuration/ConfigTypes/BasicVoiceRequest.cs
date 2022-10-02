using System;
using System.Collections.Generic;
using System.Linq;

namespace Randomizer.Data.Configuration.ConfigTypes
{
    /// <summary>
    /// Represents a basic request that supports voice recognition with a simple
    /// response.
    /// </summary>
    public class BasicVoiceRequest: IMergeable<BasicVoiceRequest>
    {
        public BasicVoiceRequest() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicVoiceRequest"/>
        /// class.
        /// </summary>
        /// <param name="phrases">The phrases that should be recognized.</param>
        /// <param name="response">The possible responses to the requests</param>
        public BasicVoiceRequest(List<string> phrases, SchrodingersString response)
        {
            Phrases = phrases;
            Response = response;
        }

        [MergeKey]
        public string Key => Phrases[0].ToLower();

        /// <summary>
        /// Gets a collection of phrases that should be recognized.
        /// </summary>
        public List<string> Phrases { get; set; } = new();

        /// <summary>
        /// Gets the possible responses to the request.
        /// </summary>
        public SchrodingersString Response { get; set; } = new();
    }
}
